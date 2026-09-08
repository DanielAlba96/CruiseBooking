using Core.Agents.Common;
using Core.Agents.Models;
using Core.Application.Models;
using Shared.Domain.Exceptions;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Core.Agents.Tools;

/// <summary>
/// Herramientas para la creación de reservas expuestas al modelo de lenguaje.
/// </summary>
internal sealed class BookingTools(
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IJobService jobService,
    ICacheService cacheService,
    UserInfo userInfo,
    Guid sessionId)
{
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IJobService _jobService = jobService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserInfo _userInfo = userInfo;

    [DisplayName(BookingToolNames.SearchCruises)]
    [Description("Busca cruceros del catálogo filtrando por zona, duración y tipo. Todos los filtros son opcionales. Devuelve una lista paginada con 5 elementos y un indicador para saber si hay más páginas")]
    public async Task<string> SearchCruises(
       [Description("Zona geográfica del itinerario. Mayor que 0")] string? zone = null,
       [Description("Duración mínima del crucero en días. Mayor que 0")] int? minDays = null,
       [Description("Duración máxima del crucero en días. Mayor que 0")] int? maxDays = null,
       [Description("Solo cruceros para adultos")] bool adultsOnly = false,
       [Description("Número de página, empezando en 1. Solo usar valores mayores que 1 si el usuario pide ver más resultados")] int page = 1)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is not null)
        {
            await _cabinRepository.ClearLockedCabins(draft.CruiseDateId, _userInfo.Id);
            await _cacheService.RemoveAsync(ChatCacheKeyReference.Draft(sessionId));
        }

        var pageSize = 5;

        var (items, hasMore) = await _cruiseRepository.SearchCruisesAsync(zone, minDays, maxDays, adultsOnly, pageSize, page);
        var reducedResults = items.Select(x => new CruiseRecord
        (
            x.Id,
            x.Name,
            x.Zone,
            x.OriginPort,
            x.Itinerary,
            x.DurationInDays,
            x.AdultsOnly,
            x.CruiseDates.ToDictionary(x => x.Id, y => y.StartDate)
         ));

        return JsonSerializer.Serialize(new
        {
            ok = true,
            has_more = hasMore,
            cruises = reducedResults
        });
    }

    [DisplayName(BookingToolNames.GetAvailableCabins)]
    [Description("Obtiene los camarotes disponibles en una fecha de salida concreta")]
    public async Task<string> GetAvailableCabins(
        [Description("Id de la fecha de salida del crucero")] int cruiseDateId)
    {
        var result = await _cabinRepository.GetCabinsByCruiseDate(cruiseDateId);
        var reducedResult = result.Select(x =>
            new CabinRecord(
                x.Id,
                x.Number,
                x.Price,
                new CabinTypeRecord(x.CabinType!.Name, x.CabinType.MaxOccupancy, x.CabinType.Description)));

        return JsonSerializer.Serialize(new
        {
            ok = true,
            cabins = reducedResult
        });
    }

    [DisplayName(BookingToolNames.GetExtras)]
    [Description("Obtiene los extras contratables en una fecha de salida concreta")]
    public async Task<string> GetExtras(
        [Description("Id de la fecha de salida del crucero")] int cruiseDateId)
    {
        var result = await _extraRepository.GetExtrasAsync(cruiseDateId);
        var reducedResult = result.Select(e => new GetExtraRecord(e.ExtraId, e.Extra!.Name, e.Extra.Description, e.Price));

        return JsonSerializer.Serialize(reducedResult);
    }

    [DisplayName(BookingToolNames.AddCabin)]
    [Description("Añade un camarote a la reserva y lo bloquea temporalmente. Si no existe reserva, la crea")]
    public async Task<string> AddCabin(
        [Description("Id del camarote")] int cabinId,
        [Description("Id de la fecha de salida del crucero")] int cruiseDateId,
        [Description("Número de pasajeros que ocuparán este camarote")] int occupants)
    {
        occupants = Math.Max(1, occupants);

        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null || draft.CruiseDateId == 0)
        {
            var cruiseDate = await _cruiseRepository.GetCruiseDateAsync(cruiseDateId);
            if (cruiseDate is null)
            {
                return JsonSerializer.Serialize(new { ok = false, error = "La fecha de salida de crucero no existe" });
            }

            draft = new BookingDraft
            {
                CruiseDateId = cruiseDate.Id,
                CruiseName = cruiseDate.Cruise!.Name,
                ShipId = cruiseDate.Ship!.Id,
                ShipName = cruiseDate.Ship!.Name,
                DepartureDate = cruiseDate.StartDate
            };
        }

        try
        {
            var cabin = await _cabinRepository.GetCabinById(cabinId);
            if (cabin is null)
                return JsonSerializer.Serialize(new { ok = false, error = "No existe un camarote con el id indicado" });

            var lockedCabinId = await _cabinRepository.LockCabin(draft.CruiseDateId, _userInfo.Id, cabin.Id, cabin.Price, occupants);
            await _jobService.ScheduleLockedCabinCleanUp([lockedCabinId]);

            draft.Cabins.Add(new DraftCabin
            {
                Id = cabin.Id,
                Name = cabin.CabinType!.Name,
                Price = cabin.Price,
                Occupants = occupants
            });

            await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

            return JsonSerializer.Serialize(new { ok = true });
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { ok = false, error = ex.Message });
        }
    }

    [DisplayName(BookingToolNames.RemoveCabin)]
    [Description("Quita un camarote de la reserva y libera su bloqueo.")]
    public async Task<string> RemoveCabin(
        [Description("Id del camarote obtenido del borrador.")] int cabinId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null || draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "El camarote no está en la reserva" });
        }

        var matches = draft.Cabins.Where(c => c.Id == cabinId).ToList();
        if (matches.Count == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "El camarote no está en la reserva" });
        }

        await _cabinRepository.UnlockCabin(draft.CruiseDateId, cabinId, _userInfo.Id);
        draft.Cabins.Remove(matches[^1]);

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true });
    }

    [DisplayName(BookingToolNames.AddExtra)]
    [Description("Añade un extra de la reserva. Si no existe reserva, la crea")]
    public async Task<string> AddExtra(
        [Description("Id del extra")] int extraId,
        [Description("Id de la fecha de salida del crucero")] int cruiseDateId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null || draft.CruiseDateId == 0)
        {
            var cruiseDate = await _cruiseRepository.GetCruiseDateAsync(cruiseDateId);
            if (cruiseDate is null)
            {
                return JsonSerializer.Serialize(new { ok = false, error = "La fecha de salida de crucero no existe" });
            }

            draft = new BookingDraft
            {
                CruiseDateId = cruiseDate.Id,
                CruiseName = cruiseDate.Cruise!.Name,
                ShipId = cruiseDate.Ship!.Id,
                ShipName = cruiseDate.Ship!.Name,
                DepartureDate = cruiseDate.StartDate
            };
        }

        if (draft.Extras.Any(e => e.ExtraId == extraId))
        {
            return JsonSerializer.Serialize(new { ok = false, error = "El extra ya está en la reserva" });
        }

        var extra = await _extraRepository.GetCruiseDateExtraAsync(draft.CruiseDateId, extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "El extra no está disponible para este crucero" });
        }

        draft.Extras.Add(new DraftExtra
        {
            ExtraId = extra.ExtraId,
            Name = extra.Extra!.Name
        });

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true });
    }

    [DisplayName(BookingToolNames.RemoveExtra)]
    [Description("Quita un extra de la reserva")]
    public async Task<string> RemoveExtra(
        [Description("Id del extra")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null || draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "El extra no está en la reserva" });
        }

        var extra = draft.Extras.FirstOrDefault(e => e.ExtraId == extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "El extra no está en la reserva" });
        }

        draft.Extras.Remove(extra);

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true });
    }

    [DisplayName(BookingToolNames.ConfirmBooking)]
    [Description("Genera un resumen de la reserva y lo deja pendiente de aprobación por parte del usuario")]
    public async Task<string> ConfirmBooking()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null || draft.Cabins.Count == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "No hay camarotes en la reserva" });
        }

        var summary = await BuildBookingSummary(draft);

        ToolApprovalRequest approvalRequest = new ConfirmBookingApprovalRequest(
            $"{BookingToolNames.ConfirmBooking}_{Guid.NewGuid()}",
            BookingToolNames.ConfirmBooking,
            summary);

        await _cacheService.SetAsync(ChatCacheKeyReference.ManualApproval(sessionId), approvalRequest, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new
        {
            ok = true,
            message = "Se ha generado resumen de la reserva para su aprobación. Responde brevemente pidiendo al usuario que lo confirme, sin incluir ninguna informacion sobre ella"
        });
    }

    private async Task<BookingConfirmationSummary> BuildBookingSummary(BookingDraft draft)
    {
        var availableExtras = await _extraRepository.GetExtrasAsync(draft.CruiseDateId);
        var extraPrices = availableExtras.ToDictionary(e => e.ExtraId, e => e.Price);

        List<SummaryCabinLine> cabins = [];
        List<SummaryExtraLine> extras = [];

        decimal total = 0.0m;

        foreach (var cabin in draft.Cabins)
        {
            total += cabin.Price;
            cabins.Add(new SummaryCabinLine(cabin.Name, cabin.Occupants, cabin.Price));
        }

        foreach (var extra in draft.Extras)
        {
            var price = extraPrices[extra.ExtraId];
            total += price;
            extras.Add(new SummaryExtraLine(extra.Name, price));
        }

        return new BookingConfirmationSummary(
            "Resumen de su reserva",
            draft.CruiseName,
            draft.ShipName,
            draft.DepartureDate,
            cabins,
            extras,
            total);
    }

    private sealed record CruiseRecord(
        int Id,
        string Name,
        string Zone,
        string OriginPort,
        string Itinerary,
        int DurationInDays,
        bool AdultsOnly,
        IDictionary<int, DateTime> DepartureDates);

    private sealed record CabinRecord(int Id, int NumberAvailable, decimal Price, CabinTypeRecord CabinType);

    private sealed record GetExtraRecord(int Id, string Name, string Description, decimal Price);

    private sealed record CabinTypeRecord(string Name, int MaxOccupancy, string Description);
}

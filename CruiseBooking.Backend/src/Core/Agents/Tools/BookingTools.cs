using Core.Agents.Common;
using Core.Agents.Models;
using Core.Application.Models;
using Shared.Domain.Exceptions;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.ComponentModel;
using System.Globalization;
using System.Text;
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

    [DisplayName("search_cruises")]
    [Description("Busca cruceros del catálogo filtrando por zona, duración y tipo. Todos los filtros son opcionales. Devuelve una lista paginada con 5 elementos y un indicador para saber si hay más páginas")]
    public async Task<string> SearchCruises(
       [Description("Zona geográfica del itinerario. Mayor que 0")] string? zone = null,
       [Description("Duración mínima del crucero en días. Mayor que 0")] int? minDays = null,
       [Description("Duración máxima del crucero en días. Mayor que 0")] int? maxDays = null,
       [Description("Solo cruceros para adultos")] bool adultsOnly = false,
       [Description("Número de página, empezando en 1. Solo usar valores mayores que 1 si el usuario pide ver más resultados")] int page = 1)
    {
        var pageSize = 5;

        var (items, hasMore) = await _cruiseRepository.SearchCruisesAsync(zone, minDays, maxDays, adultsOnly, pageSize, page);
        var reducedResults = items.Select(x => new CruiseRecord
        (
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

    [DisplayName("get_available_cabins")]
    [Description("Obtiene las cabinas disponibles en una fecha de salida concreta")]
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

    [DisplayName("get_extras")]
    [Description("Obtiene los extras contratables en una fecha de salida concreta")]
    public async Task<string> GetExtras(
        [Description("Id de la fecha de salida del crucero")] int cruiseDateId)
    {
        var result = await _extraRepository.GetExtrasAsync(cruiseDateId);
        var reducedResult = result.Select(e => new GetExtraRecord(e.ExtraId, e.Extra!.Name, e.Extra.Description, e.Price));

        return JsonSerializer.Serialize(reducedResult);
    }

    [DisplayName("get_booking_draft")]
    [Description("Devuelve el borrador de reserva en curso")]
    public async Task<string> GetBookingDraft()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null || draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { ok = true, has_draft = false });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            has_draft = true,
            draft
        });
    }

    [DisplayName("start_booking")]
    [Description("Crea el borrador de reserva para una fecha de salida concreta")]
    public async Task<string> StartBooking(
        [Description("Id de la fecha de salida del crucero")] int cruiseDateId)
    {
        var cruiseDate = await _cruiseRepository.GetCruiseDateAsync(cruiseDateId);
        if (cruiseDate is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La fecha de salida de crucero no existe" });
        }

        var draft = new BookingDraft
        {
            CruiseDateId = cruiseDate.Id,
            CruiseName = cruiseDate.Cruise!.Name,
            ShipId = cruiseDate.Ship!.Id,
            ShipName = cruiseDate.Ship!.Name,
            DepartureDate = cruiseDate.StartDate
        };

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true });
    }

    [DisplayName("add_cabin")]
    [Description("Añade un camarote al borrador y lo bloquea temporalmente")]
    public async Task<string> AddCabin(
        [Description("Id del camarote")] int cabinId,
        [Description("Número de pasajeros que ocuparán este camarote")] int occupants)
    {
        occupants = Math.Max(1, occupants);

        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "Primero inicia la reserva con start_booking" });
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

    [DisplayName("remove_cabin")]
    [Description("Quita un camarote del borrador y libera su bloqueo.")]
    public async Task<string> RemoveCabin(
        [Description("Id de la cabina obtenido del borrador.")] int cabinId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        var matches = draft.Cabins.Where(c => c.Id == cabinId).ToList();
        if (matches.Count == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La cabina no está en la reserva" });
        }

        await _cabinRepository.UnlockCabin(draft.CruiseDateId, cabinId, _userInfo.Id);
        draft.Cabins.Remove(matches[^1]);

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true });
    }

    [DisplayName("add_extra")]
    [Description("Añade un extra al borrador")]
    public async Task<string> AddExtra(
        [Description("Id del extra")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "Primero inicia la reserva con start_booking" });
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

    [DisplayName("remove_extra")]
    [Description("Quita un extra del borrador")]
    public async Task<string> RemoveExtra(
        [Description("Id del extra tal como aparece en el borrador")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
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

    [DisplayName("restart_booking")]
    [Description("Descarta el borrador entero y libera las cabinas bloqueadas. Es el único modo de cambiar de crucero o de fecha de salida, porque el borrador está ligado a una salida concreta. Destructiva e irreversible.")]
    public async Task<string> RestartBooking()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "No hay ninguna reserva iniciada" });
        }

        await _cabinRepository.ClearLockedCabins(draft.CruiseDateId, _userInfo.Id);

        await _cacheService.RemoveAsync(ChatCacheKeyReference.Draft(sessionId));

        return JsonSerializer.Serialize(new { ok = true, message = "Reserva reiniciada. Empieza de nuevo con start_booking." });
    }

    [DisplayName("confirm_booking")]
    [Description("Finaliza la reserva generando un borrador final de la misma y lo deja pendiente de aprobación por parte del usuario")]
    public async Task<string> ConfirmBooking()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.Cabins.Count == 0)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "No hay camarotes en la reserva" });
        }

        var summary = await BuildBookingSummary(draft);

        ToolApprovalRequest approvalRequest = new ConfirmBookingApprovalRequest(
            $"{ApprovalToolNames.ConfirmBooking}_{Guid.NewGuid()}",
            ApprovalToolNames.ConfirmBooking,
            summary);

        await _cacheService.SetAsync(ChatCacheKeyReference.ManualApproval(sessionId), approvalRequest, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new
        {
            ok = true,
            message = "Se ha generado el borrador final de la reserva. Responde brevemente pidiendo al usuario que lo revise y confirme, sin incluir un resumen de la reserva. Aun no se ha generado la reserva en el sistema. El usuario te avisará cuando la reserva sea aprobada o rechazada."
        });
    }

    private async Task<string> BuildBookingSummary(BookingDraft draft)
    {
        var availableExtras = await _extraRepository.GetExtrasAsync(draft.CruiseDateId);
        var extraPrices = availableExtras.ToDictionary(e => e.ExtraId, e => e.Price);

        decimal total = 0.0m;

        var culture = CultureInfo.GetCultureInfo("es-ES");
        var sb = new StringBuilder();

        sb.AppendLine("# Resumen de su reserva");
        sb.AppendLine();

        sb.AppendLine("## Camarotes");
        sb.AppendLine();

        if (draft.Cabins.Count > 0)
        {
            sb.AppendLine("| Camarote | Nº Pasajeros | Precio |");
            sb.AppendLine("| --- | --- | --- |");

            foreach (var cabin in draft.Cabins)
            {
                total += cabin.Price;
                sb.AppendLine($"| {cabin.Name} | {cabin.Occupants} | {cabin.Price.ToString("C", culture)} |");
            }
        }
        else
        {
            sb.AppendLine("_Sin camarotes seleccionados._");
        }

        sb.AppendLine();
        sb.AppendLine("## Extras");
        sb.AppendLine();

        if (draft.Extras.Count > 0)
        {
            sb.AppendLine("| Extra | Precio |");
            sb.AppendLine("| --- | --- |");

            foreach (var extra in draft.Extras)
            {
                total += extraPrices[extra.ExtraId];
                sb.AppendLine($"| {extra.Name} | {extraPrices[extra.ExtraId].ToString("C", culture)} |");
            }
        }
        else
        {
            sb.AppendLine("_Sin extras seleccionados._");
        }

        sb.AppendLine();
        sb.AppendLine($"**Total: {total.ToString("C", culture)}**");

        return sb.ToString();
    }

    private sealed record CruiseRecord(
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

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
    [Description("Busca cruceros del catálogo filtrando por zona, duración y tipo. Devuelve para cada crucero su id, nombre, zona y duración. El id resultante es el que espera get_cruise_dates. Todos los filtros son opcionales: omítelos para ver el catálogo completo.")]
    public async Task<string> SearchCruises(
   [Description("Zona geográfica del itinerario (Caribe, Mediterráneo, Norte de Europa...). Es lo que suele indicar el usuario, en lugar del nombre del crucero")] string? zone,
   [Description("Duración mínima del crucero en días")] int? minDays,
   [Description("Duración máxima del crucero en días")] int? maxDays,
   [Description("true para devolver únicamente cruceros solo para adultos")] bool adultsOnly)
    {
        var results = await _cruiseRepository.SearchCruisesAsync(zone, minDays, maxDays, adultsOnly);
        return JsonSerializer.Serialize(results);
    }

    [DisplayName("get_cruise_dates")]
    [Description("Obtiene las fechas de salida disponibles de un crucero concreto. Cada salida devuelve su propio id (cruise_date_id), distinto del id del crucero: es el que esperan get_available_cabins, get_extras y start_booking. Las cabinas, los extras y sus precios dependen de la salida elegida, no del crucero.")]
    public async Task<string> GetCruiseDates(
        [Description("ID del crucero obtenido de search_cruises")] int cruiseId)
    {
        var result = await _cruiseRepository.GetCruiseDatesAsync(cruiseId);
        return JsonSerializer.Serialize(result);
    }

    [DisplayName("get_available_cabins")]
    [Description("Obtiene las cabinas disponibles en una fecha de salida concreta. Cada cabina devuelve su id, su precio por camarote (independiente del número de pasajeros) y su tipo, con el nombre y el aforo máximo (maxOccupancy) en cabinType. Son los valores que espera add_cabin: no los inventes ni los deduzcas de otra fuente.")]
    public async Task<string> GetAvailableCabins(
        [Description("ID de la fecha de salida del crucero obtenido de get_cruise_dates")] int cruiseDateId)
    {
        var result = await _cabinRepository.GetCabinsByCruiseDate(cruiseDateId);
        return JsonSerializer.Serialize(result);
    }

    [DisplayName("get_extras")]
    [Description("Obtiene los extras contratables en una fecha de salida concreta: id, nombre, descripción y precio de cada uno. El extra_id devuelto es el que espera add_extra.")]
    public async Task<string> GetExtras(
        [Description("ID de la fecha de salida del crucero obtenido de get_cruise_dates")] int cruiseDateId)
    {
        var extras = await _extraRepository.GetExtrasAsync(cruiseDateId);
        var result = extras.Select(e => new { extra_id = e.ExtraId, name = e.Extra!.Name, description = e.Extra!.Description, price = e.Price });

        return JsonSerializer.Serialize(result);
    }

    [DisplayName("get_booking_draft")]
    [Description("Devuelve el borrador de reserva en curso: crucero, barco, fecha de salida, cabinas añadidas con sus ocupantes y precios, y extras contratados. Si has_draft es false no hay ninguna reserva empezada. Llámala al inicio de la conversación para saber si el usuario dejó una reserva a medias, y siempre que necesites consultar el estado actual en lugar de fiarte de la conversación.")]
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
    [Description("Crea el borrador de reserva para una fecha de salida concreta. Es obligatoria antes de add_cabin y add_extra. Descarta sin aviso cualquier borrador anterior, así que si ya hay uno en curso confirma antes con el usuario.")]
    public async Task<string> StartBooking(
        [Description("ID de la fecha de salida del crucero obtenido de get_cruise_dates")] int cruiseDateId)
    {
        var cruiseDate = await _cruiseRepository.GetCruiseDateAsync(cruiseDateId);
        if (cruiseDate is null)
        {
            return JsonSerializer.Serialize(new { error = "La fecha de salida de crucero no existe" });
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

        return JsonSerializer.Serialize(new { ok = true, cruise_date_id = cruiseDate.Id });
    }

    [DisplayName("add_cabin")]
    [Description("Añade UN camarote al borrador y lo bloquea temporalmente. Requiere start_booking previo. Para reservar varios camarotes del mismo tipo, llama una vez por camarote, indicando en cada llamada los ocupantes de ese camarote concreto.")]
    public async Task<string> AddCabin(
        [Description("ID de la cabina obtenido de get_available_cabins. Nunca un número dicho por el usuario: no conoce los IDs")] int cabinId,
        [Description("Nombre de la cabina tal cual viene en cabinType de get_available_cabins")] string cabinName,
        [Description("Precio de la cabina obtenido de get_available_cabins, sin recalcular ni prorratear por pasajero")] decimal price,
        [Description("Número de pasajeros que ocuparán este camarote. No puede superar el maxOccupancy de su tipo. Pregúntaselo al usuario antes de llamar; es solo la cantidad de personas, nunca sus datos personales")] int occupants)
    {
        occupants = Math.Max(1, occupants);

        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "Primero inicia la reserva con start_booking" });
        }
            try
            {
                var lockedCabinId = await _cabinRepository.LockCabin(draft.CruiseDateId, _userInfo.Id, cabinId, price, occupants);
                await _jobService.ScheduleLockedCabinCleanUp([lockedCabinId]);
            }
            catch (ControlledException ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, locked = draft.Cabins.Count(c => c.Id == cabinId) });
            }

            draft.Cabins.Add(new DraftCabin
            {
                Id = cabinId,
                Name = cabinName,
                Price = price,
                Occupants = occupants
            });

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true, cabinId, price, occupants });
    }

    [DisplayName("remove_cabin")]
    [Description("Quita del borrador UN camarote de este tipo y libera su bloqueo. Si hay varios del mismo tipo, elimina solo uno y devuelve en remaining cuántos quedan: repite la llamada para quitar más.")]
    public async Task<string> RemoveCabin(
        [Description("ID de la cabina obtenido de get_available_cabins o del borrador. Nunca un número dicho por el usuario")] int cabinId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        var matches = draft.Cabins.Where(c => c.Id == cabinId).ToList();
        if (matches.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "La cabina no está en la reserva" });
        }

        await _cabinRepository.UnlockCabin(draft.CruiseDateId, cabinId, _userInfo.Id);
        draft.Cabins.Remove(matches[^1]);

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        var remaining = draft.Cabins.Count(c => c.Id == cabinId);

        return JsonSerializer.Serialize(new { ok = true, cabin_id = cabinId, remaining });
    }

    [DisplayName("add_extra")]
    [Description("Añade un extra al borrador. Requiere start_booking previo. Cada extra se contrata una sola vez para toda la reserva: no lo añadas por cabina ni por pasajero.")]
    public async Task<string> AddExtra(
        [Description("ID del extra obtenido de get_extras para esta misma fecha de salida")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "Primero inicia la reserva con start_booking" });
        }

        if (draft.Extras.Any(e => e.ExtraId == extraId))
        {
            return JsonSerializer.Serialize(new { error = "El extra ya está en la reserva" });
        }

        var extra = await _extraRepository.GetCruiseDateExtraAsync(draft.CruiseDateId, extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { error = "El extra no está disponible para este crucero" });
        }

        draft.Extras.Add(new DraftExtra
        {
            ExtraId = extra.ExtraId,
            Name = extra.Extra!.Name
        });

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true, extra_id = extra.ExtraId, name = extra.Extra!.Name });
    }

    [DisplayName("remove_extra")]
    [Description("Quita un extra del borrador de reserva.")]
    public async Task<string> RemoveExtra(
        [Description("ID del extra tal como aparece en el borrador")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        var extra = draft.Extras.FirstOrDefault(e => e.ExtraId == extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { error = "El extra no está en la reserva" });
        }

        draft.Extras.Remove(extra);

        await _cacheService.SetAsync(ChatCacheKeyReference.Draft(sessionId), draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true, extra_id = extraId });
    }

    [DisplayName("restart_booking")]
    [Description("Descarta el borrador entero y libera las cabinas bloqueadas. Es el único modo de cambiar de crucero o de fecha de salida, porque el borrador está ligado a una salida concreta. Destructiva e irreversible: llámala SOLO tras la confirmación explícita del usuario, nunca para modificar cabinas o extras (usa remove_cabin y remove_extra).")]
    public async Task<string> RestartBooking()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId));
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "No hay ninguna reserva iniciada" });
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
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.Cabins.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "No hay camarotes en la reserva" });
        }

        var summary = await BuildBookingSummary(draft);

        // El tipo declarado debe ser la base para que la serialización escriba el discriminador.
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
}

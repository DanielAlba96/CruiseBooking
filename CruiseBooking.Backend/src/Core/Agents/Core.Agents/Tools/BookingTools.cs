using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
using Shared.Domain.Exceptions;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Core.Agents.Tools;

/// <summary>
/// Herramientas de manejo del borrador de reserva expuestas al modelo de lenguaje.
/// Las dependencias y el borrador se reciben por constructor para que cada método declare únicamente
/// los parámetros que rellena el modelo; así <c>AIFunctionFactory</c> conserva el nombre del método y los
/// <see cref="DescriptionAttribute"/> de la función y de cada parámetro en el esquema JSON.
/// </summary>
internal sealed class BookingTools(
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IJobService jobService,
    ICacheService cacheService,
    UserInfo userInfo,
    IMediator mediator,
    Guid sessionId)
{
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IJobService _jobService = jobService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserInfo _userInfo = userInfo;
    private readonly IMediator _mediator = mediator;

    private readonly string _draftkey = $"chat:{sessionId}:draft";

    [DisplayName("get_booking_draft")]
    [Description("Devuelve el borrador de reserva en curso: crucero, barco, fecha de salida, cabinas añadidas con sus ocupantes y precios, y extras contratados. Si has_draft es false no hay ninguna reserva empezada. Llámala al inicio de la conversación para saber si el usuario dejó una reserva a medias, y siempre que necesites consultar el estado actual en lugar de fiarte de la conversación.")]
    public async Task<string> GetBookingDraft()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
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
    [Description("Crea el borrador de reserva para una fecha de salida concreta. Es obligatoria antes de add_cabin y add_extra. Descarta sin aviso cualquier borrador anterior, así que si ya hay uno en curso confirma antes con el usuario. El borrador queda ligado a esta salida: para cambiar de crucero o de fecha hay que llamar a restart_booking y empezar de nuevo.")]
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

        await _cacheService.SetAsync(_draftkey, draft, TimeSpan.FromMinutes(10));

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

        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
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

        await _cacheService.SetAsync(_draftkey, draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true, cabinId, price, occupants });
    }

    [DisplayName("remove_cabin")]
    [Description("Quita del borrador UN camarote de este tipo y libera su bloqueo. Si hay varios del mismo tipo, elimina solo uno y devuelve en remaining cuántos quedan: repite la llamada para quitar más.")]
    public async Task<string> RemoveCabin(
        [Description("ID de la cabina obtenido de get_available_cabins o del borrador. Nunca un número dicho por el usuario")] int cabinId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
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

        await _cacheService.SetAsync(_draftkey, draft, TimeSpan.FromMinutes(10));

        var remaining = draft.Cabins.Count(c => c.Id == cabinId);

        return JsonSerializer.Serialize(new { ok = true, cabin_id = cabinId, remaining });
    }

    [DisplayName("add_extra")]
    [Description("Añade un extra al borrador. Requiere start_booking previo. Cada extra se contrata una sola vez para toda la reserva: no lo añadas por cabina ni por pasajero.")]
    public async Task<string> AddExtra(
        [Description("ID del extra obtenido de get_extras para esta misma fecha de salida")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
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

        await _cacheService.SetAsync(_draftkey, draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true, extra_id = extra.ExtraId, name = extra.Extra!.Name });
    }

    [DisplayName("remove_extra")]
    [Description("Quita un extra del borrador de reserva.")]
    public async Task<string> RemoveExtra(
        [Description("ID del extra tal como aparece en el borrador")] int extraId)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
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

        await _cacheService.SetAsync(_draftkey, draft, TimeSpan.FromMinutes(10));

        return JsonSerializer.Serialize(new { ok = true, extra_id = extraId });
    }

    [DisplayName("restart_booking")]
    [Description("Descarta el borrador entero y libera las cabinas bloqueadas. Es el único modo de cambiar de crucero o de fecha de salida, porque el borrador está ligado a una salida concreta. Destructiva e irreversible: llámala SOLO tras la confirmación explícita del usuario, nunca para modificar cabinas o extras (usa remove_cabin y remove_extra).")]
    public async Task<string> RestartBooking()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "No hay ninguna reserva iniciada" });
        }

        await _cabinRepository.ClearLockedCabins(draft.CruiseDateId, _userInfo.Id);

        await _cacheService.RemoveAsync(_draftkey);

        return JsonSerializer.Serialize(new { ok = true, message = "Reserva reiniciada. Empieza de nuevo con start_booking." });
    }

    [DisplayName("confirm_booking")]
    [Description("Cierra la reserva definitivamente y la guarda. Requiere al menos una cabina en el borrador. Al invocarla se muestra al usuario un resumen que debe aprobar, así que avísale antes de llamarla. Es el único modo de convertir el borrador en una reserva real: no des la reserva por hecha hasta que devuelva ok.")]
    public async Task<string> ConfirmBooking()
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(_draftkey);
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "La reserva ha expirado, vuelve a iniciarla con start_booking" });
        }

        if (draft.Cabins.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "No hay cabinas en la reserva" });
        }

        var bookingDto = new CreateBookingRequest
        {
            CruiseDateId = draft.CruiseDateId,
            SelectedCabins = [.. draft.Cabins.Select(c => new BookingCabinSelection(c.Id, c.Occupants, c.Price))],
            SelectedExtras = [.. draft.Extras.Select(e => e.ExtraId)]
        };

        try
        {
            await _mediator.Send(new CompleteBooking(bookingDto));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        await _cacheService.RemoveAsync(_draftkey);

        return JsonSerializer.Serialize(new
        {
            ok = true,
            check_in = "El usuario recibirá un email con la factura y un enlace para completar el check-in online con los datos de los pasajeros antes de la salida."
        });
    }
}

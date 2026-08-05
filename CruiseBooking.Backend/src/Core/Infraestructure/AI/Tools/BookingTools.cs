using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
using Core.Infrastructure.AI.Models;
using Shared.Domain.Exceptions;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.ComponentModel;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

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
    IMediator mediator,
    IJobService jobService,
    UserInfo userInfo,
    BookingDraft draft)
{
    [Description("Inicia un borrador de reserva para una fecha de salida de crucero. Reinicia cualquier borrador anterior.")]
    public async Task<string> StartBooking(
        [Description("ID de la fecha de salida del crucero")] int cruiseDateId)
    {
        var cruiseDate = await cruiseRepository.GetCruiseDateAsync(cruiseDateId);
        if (cruiseDate is null)
        {
            return JsonSerializer.Serialize(new { error = "La fecha de salida de crucero no existe" });
        }

        draft.CruiseDateId = cruiseDate.Id;
        draft.ShipId = cruiseDate.ShipId;

        return JsonSerializer.Serialize(new { ok = true, cruise_date_id = cruiseDate.Id });
    }

    [Description("Añade una o varias cabinas del mismo tipo al borrador de reserva. Requiere haber iniciado la reserva con start_booking.")]
    public async Task<string> AddCabin(
        [Description("ID de la cabina del barco")] int cabinId,
        [Description("Precio de la cabina obtenido de get_available_cabins")] decimal price,
        [Description("Aforo máximo de la cabina obtenido de get_available_cabins")] int maxOccupancy,
        [Description("Número de pasajeros que ocuparán cada cabina de este tipo (no puede superar max_occupancy)")] int occupants,
        [Description("Número de cabinas de este tipo a añadir (por defecto 1)")] int quantity)
    {
        occupants = Math.Max(1, occupants);
        quantity = Math.Max(1, quantity);

        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "Primero inicia la reserva con start_booking" });
        }

        for (int i = 0; i < quantity; i++)
        {
            try
            {
                var lockedCabinId = await cabinRepository.LockCabin(draft.CruiseDateId, userInfo.Id, cabinId, price, occupants);
                await jobService.ScheduleLockedCabinCleanUp([lockedCabinId]);
            }
            catch (ControlledException ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, locked = draft.Cabins.Count(c => c.CabinId == cabinId) });
            }

            draft.Cabins.Add(new DraftCabin
            {
                CabinId = cabinId,
                Price = price,
                MaxOccupancy = maxOccupancy,
                Occupants = occupants
            });
        }

        var totalOfType = draft.Cabins.Count(c => c.CabinId == cabinId);

        return JsonSerializer.Serialize(new { ok = true, cabin_id = cabinId, price, max_occupancy = maxOccupancy, quantity, total_of_type = totalOfType });
    }

    [Description("Quita una cabina del borrador de reserva.")]
    public async Task<string> RemoveCabin(
        [Description("ID de la cabina del barco")] int cabinId)
    {
        var matches = draft.Cabins.Where(c => c.CabinId == cabinId).ToList();
        if (matches.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "La cabina no está en la reserva" });
        }

        await cabinRepository.UnlockCabin(draft.CruiseDateId, cabinId, userInfo.Id);
        draft.Cabins.Remove(matches[^1]);

        var remaining = draft.Cabins.Count(c => c.CabinId == cabinId);

        return JsonSerializer.Serialize(new { ok = true, cabin_id = cabinId, remaining });
    }

    [Description("Añade un extra al borrador de reserva. Requiere haber iniciado la reserva con start_booking.")]
    public async Task<string> AddExtra(
        [Description("ID del extra")] int extraId)
    {
        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "Primero inicia la reserva con start_booking" });
        }

        if (draft.Extras.Any(e => e.ExtraId == extraId))
        {
            return JsonSerializer.Serialize(new { error = "El extra ya está en la reserva" });
        }

        var extra = await extraRepository.GetCruiseDateExtraAsync(draft.CruiseDateId, extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { error = "El extra no está disponible para este crucero" });
        }

        draft.Extras.Add(new DraftExtra
        {
            ExtraId = extra.ExtraId,
            Name = extra.Extra!.Name,
            Price = extra.Price
        });

        return JsonSerializer.Serialize(new { ok = true, extra_id = extra.ExtraId, name = extra.Extra!.Name, price = extra.Price });
    }

    [Description("Quita un extra del borrador de reserva.")]
    public string RemoveExtra(
        [Description("ID del extra")] int extraId)
    {
        var extra = draft.Extras.FirstOrDefault(e => e.ExtraId == extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { error = "El extra no está en la reserva" });
        }

        draft.Extras.Remove(extra);

        return JsonSerializer.Serialize(new { ok = true, extra_id = extraId });
    }

    [Description("Devuelve el resumen del borrador de reserva con cabinas, número de ocupantes, extras y el precio total, y bloquea las cabinas con los precios mostrados.")]
    public async Task<string> GetBookingSummary()
    {
        if (draft.Cabins.Count == 0 && draft.Extras.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "El borrador de reserva está vacío" });
        }

        var availableCabins = await cabinRepository.GetCabinsByCruiseDate(draft.CruiseDateId);
        var cabinPrices = availableCabins.ToDictionary(c => c.Id, c => c.Price);
        var availableExtras = await extraRepository.GetExtrasAsync(draft.CruiseDateId);
        var extraPrices = availableExtras.ToDictionary(e => e.ExtraId, e => e.Price);

        if (draft.Cabins.Any(c => !cabinPrices.ContainsKey(c.CabinId)))
        {
            return JsonSerializer.Serialize(new { error = "Alguna de las cabinas del borrador ya no existe en esta fecha de salida. Pide al usuario que la elimine con remove_cabin o que reinicie la reserva con restart_booking." });
        }

        if (draft.Extras.Any(e => !extraPrices.ContainsKey(e.ExtraId)))
        {
            return JsonSerializer.Serialize(new { error = "Alguno de los extras del borrador ya no existe en esta fecha de salida. Pide al usuario que lo elimine con remove_extra o que reinicie la reserva con restart_booking." });
        }

        var cabins = draft.Cabins.GroupBy(c => c.CabinId).Select(g =>
        {
            var unitPrice = cabinPrices[g.Key];
            return new
            {
                cabinId = g.Key,
                quantity = g.Count(),
                unitPrice,
                lineTotal = unitPrice * g.Count(),
                maxOccupancy = g.First().MaxOccupancy
            };
        }).ToList();

        var extras = draft.Extras.Select(e => new { extraId = e.ExtraId, e.Name, price = extraPrices[e.ExtraId] }).ToList();

        var total = cabins.Sum(c => c.lineTotal) + extras.Sum(e => e.price);

        return JsonSerializer.Serialize(new { cabins, extras, total });
    }

    [Description("Reinicia la reserva desde cero: libera las cabinas bloqueadas y descarta el borrador completo. Llamar SOLO tras la confirmación explícita del usuario.")]
    public async Task<string> RestartBooking()
    {
        if (draft.CruiseDateId == 0)
        {
            return JsonSerializer.Serialize(new { error = "No hay ninguna reserva iniciada" });
        }

        await cabinRepository.ClearLockedCabins(draft.CruiseDateId, userInfo.Id);
        draft.Clear();

        return JsonSerializer.Serialize(new { ok = true, message = "Reserva reiniciada. Empieza de nuevo con start_booking." });
    }

    [Description("Confirma la reserva: cobra con el método de pago del usuario y la guarda. Llamar solo tras la confirmación explícita del usuario sobre el resumen.")]
    public async Task<string> ConfirmBooking()
    {
        if (draft.Cabins.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "No hay cabinas en la reserva" });
        }

        var bookingDto = new CreateBookingRequest
        {
            CruiseDateId = draft.CruiseDateId,
            SelectedCabins = [.. draft.Cabins.Select(c => new BookingCabinSelection(c.CabinId, c.Occupants, c.Price))],
            SelectedExtras = [.. draft.Extras.Select(e => e.ExtraId)]
        };

        try
        {
            await mediator.Send(new CompleteBooking(bookingDto));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        draft.Clear();

        return JsonSerializer.Serialize(new
        {
            ok = true,
            check_in = "El usuario recibirá un email con la factura y un enlace para completar el check-in online con los datos de los pasajeros antes de la salida."
        });
    }
}

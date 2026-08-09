using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Entities;
using Shared.Domain.Exceptions;
using Shared.Domain.Models;
using System.ComponentModel;
using System.Text.Json;
using Core.Application.UseCases.Bookings;
using Core.Application.UseCases.Payments;

namespace Core.Agents.Tools;

/// <summary>
/// Herramientas de posventa sobre las reservas ya existentes del usuario, expuestas al modelo de lenguaje.
/// </summary>
internal sealed class PostSalesTools(IMediator mediator)
{
    private readonly IMediator _mediator = mediator;

    [DisplayName("get_my_bookings")]
    [Description("Devuelve todas las reservas del usuario: su id, el crucero, el destino, las fechas, el número de pasajeros y su estado. Incluye can_pay y can_cancel, que indican si esa reserva admite pago manual o cancelación. Llámala siempre antes de operar sobre una reserva: es la única forma de saber qué reservas existen y cuál es su id.")]
    public async Task<string> GetMyBookings()
    {
        IReadOnlyList<GetCurrentUserBookingSummaryDtoResponse> bookings;
        try
        {
            bookings = await _mediator.Send(new GetBookingsSummary());
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        var result = bookings.Select(b => new
        {
            booking_id = b.Id,
            cruise_name = b.Name,
            destination = b.Destination,
            start_date = b.StartDate.ToString("yyyy-MM-dd"),
            end_date = b.EndDate.ToString("yyyy-MM-dd"),
            passenger_count = b.PassengerCount,
            status = Enum.GetName(b.Status),
            can_pay = b.Status is BookingStatus.Created or BookingStatus.PendingPayment,
            can_cancel = b.Status != BookingStatus.Canceled
        });

        return JsonSerializer.Serialize(new { ok = true, bookings = result });
    }

    [DisplayName("get_booking_detail")]
    [Description("Devuelve el detalle de UNA reserva del usuario: barco, itinerario, camarotes con sus ocupantes y precios, extras contratados e importe total. Úsala cuando el usuario pregunte por los detalles de una reserva concreta o antes de pagarla o cancelarla, para poder confirmarle sobre qué está operando.")]
    public async Task<string> GetBookingDetail(
        [Description("ID de la reserva obtenido de get_my_bookings. Nunca un número dicho por el usuario: no conoce los IDs")] int bookingId)
    {
        GetCurrentUserBookingDetailDtoResponse booking;
        try
        {
            booking = await _mediator.Send(new GetBookingDetail(bookingId));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        var total = booking.Cabins.Sum(c => c.Price) + booking.Extras.Sum(e => e.Price);

        return JsonSerializer.Serialize(new
        {
            ok = true,
            booking_id = booking.Id,
            cruise_name = booking.Name,
            destination = booking.Destination,
            origin_port = booking.OriginPort,
            itinerary = booking.Itinerary,
            ship_name = booking.ShipName,
            start_date = booking.StartDate.ToString("yyyy-MM-dd"),
            end_date = booking.EndDate.ToString("yyyy-MM-dd"),
            duration_in_days = booking.DurationInDays,
            status = Enum.GetName(booking.Status),
            can_pay = booking.Status is BookingStatus.Created or BookingStatus.PendingPayment,
            can_cancel = booking.Status != BookingStatus.Canceled,
            is_charged = booking.ChargedAt.HasValue,
            cabins = booking.Cabins.Select(c => new
            {
                name = c.TypeName,
                occupants = c.Occupants,
                price = c.Price
            }),
            extras = booking.Extras.Select(e => new
            {
                name = e.Name,
                price = e.Price
            }),
            total
        });
    }

    [DisplayName("get_payment_methods")]
    [Description("Devuelve las tarjetas que el usuario ya tiene dadas de alta en su cuenta, con su marca, sus últimos cuatro dígitos y su caducidad. Es la única fuente válida del payment_method_id que espera pay_booking. No existe ninguna herramienta para dar de alta, modificar ni borrar tarjetas: si la lista viene vacía, el pago no se puede hacer desde la conversación y el usuario debe añadir la tarjeta desde su perfil en la web.")]
    public async Task<string> GetPaymentMethods()
    {
        List<PaymentMethodResponse> paymentMethods;
        try
        {
            paymentMethods = await _mediator.Send(new GetPaymentMethods());
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        if (paymentMethods.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                ok = true,
                payment_methods = Array.Empty<object>(),
                message = "El usuario no tiene ninguna tarjeta dada de alta. Debe añadirla desde su perfil en la web: no puedes hacerlo tú ni pedirle los datos."
            });
        }

        var result = paymentMethods.Select(p => new
        {
            payment_method_id = p.Id,
            brand = p.Card.Brand,
            last4 = p.Card.Last4,
            expires = $"{p.Card.ExpMonth:00}/{p.Card.ExpYear}"
        });

        return JsonSerializer.Serialize(new { ok = true, payment_methods = result });
    }

    [DisplayName("pay_booking")]
    [Description("Cobra manualmente una reserva pendiente de pago usando una tarjeta que el usuario ya tiene dada de alta. Requiere can_pay a true. Al invocarla se muestra al usuario un resumen que debe aprobar, así que avísale antes de llamarla. Destructiva e irreversible: mueve dinero real. El payment_method_id debe salir siempre de get_payment_methods; jamás lo construyas ni lo deduzcas de datos de tarjeta dictados por el usuario.")]
    public async Task<string> PayBooking(
        [Description("ID de la reserva obtenido de get_my_bookings. Nunca un número dicho por el usuario: no conoce los IDs")] int bookingId,
        [Description("Identificador del método de pago tal cual viene en payment_method_id de get_payment_methods. Nunca un número de tarjeta ni un valor inventado")] string paymentMethodId)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
        {
            return JsonSerializer.Serialize(new { error = "Falta el método de pago. Llama antes a get_payment_methods y pide al usuario que elija una de sus tarjetas." });
        }

        List<PaymentMethodResponse> paymentMethods;
        try
        {
            paymentMethods = await _mediator.Send(new GetPaymentMethods());
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        if (!paymentMethods.Any(p => p.Id == paymentMethodId))
        {
            return JsonSerializer.Serialize(new { error = "Ese método de pago no está dado de alta en la cuenta del usuario. Usa únicamente uno de los que devuelve get_payment_methods." });
        }

        try
        {
            await _mediator.Send(new PayBooking(bookingId, new PayBookingRequest(paymentMethodId)));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            booking_id = bookingId,
            message = "Reserva cobrada. El usuario recibirá la factura por email y ya puede completar el check-in online."
        });
    }

    [DisplayName("cancel_booking")]
    [Description("Cancela una reserva del usuario. Requiere can_cancel a true. Si la reserva ya estaba cobrada se emite además el reembolso, y solo se permite mientras no haya empezado el periodo de check-in. Al invocarla se muestra al usuario un resumen que debe aprobar, así que avísale antes de llamarla. Destructiva e irreversible: no la ejecutes de forma especulativa ni sin la confirmación explícita del usuario.")]
    public async Task<string> CancelBooking(
        [Description("ID de la reserva obtenido de get_my_bookings. Nunca un número dicho por el usuario: no conoce los IDs")] int bookingId)
    {
        try
        {
            await _mediator.Send(new CancelBooking(bookingId));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            booking_id = bookingId,
            message = "Reserva cancelada. Si estaba cobrada, el reembolso se ha emitido y tardará unos días en aparecer en la tarjeta."
        });
    }
}

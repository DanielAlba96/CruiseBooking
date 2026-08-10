using Core.Agents.Common;
using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
using Core.Application.UseCases.Payments;
using Shared.Domain.Entities;
using Shared.Domain.Exceptions;
using Shared.Domain.Models;
using Shared.Domain.Services;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Core.Agents.Tools;

/// <summary>
/// Herramientas de gestion de reservas existentes expuestas al modelo de lenguaje.
/// </summary>
internal sealed class PostSalesTools(IMediator mediator, ICacheService cacheService, Guid sessionId)
{
    private readonly IMediator _mediator = mediator;
    private readonly ICacheService _cacheService = cacheService;

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
    [Description("Genera un resumen del proceso de pago manual de una reserva existente para que el usario lo apruebe. Requiere can_pay a true")]
    public async Task<string> PayBooking(
        [Description("ID de la reserva obtenido de get_my_bookings. Nunca un número dicho por el usuario: no conoce los IDs")] int bookingId,
        [Description("Identificador del método de pago tal cual viene en payment_method_id de get_payment_methods. Nunca un número de tarjeta ni un valor inventado")] string paymentMethodId)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
        {
            return JsonSerializer.Serialize(new { error = "Falta el método de pago. Llama antes a get_payment_methods y pide al usuario que elija una de sus tarjetas." });
        }

        try
        {
            var paymentMethods = await _mediator.Send(new GetPaymentMethods());
            var paymentMethod = paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
            if (paymentMethod is null)
            {
                return JsonSerializer.Serialize(new { error = "Ese método de pago no está dado de alta en la cuenta del usuario. Usa únicamente uno de los que devuelve get_payment_methods." });
            }

            var booking = await _mediator.Send(new GetBookingDetail(bookingId));
            var summary = BuildPaymentSummary(booking, paymentMethod);

            ToolApprovalRequest approvalRequest = new PayApprovalRequest(
                $"{ApprovalToolNames.PayBooking}_{Guid.NewGuid()}",
                ApprovalToolNames.PayBooking,
                summary,
                bookingId,
                paymentMethodId);

            await _cacheService.SetAsync(ChatCacheKeyReference.ManualApproval(sessionId), approvalRequest, TimeSpan.FromMinutes(10));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            booking_id = bookingId,
            message = "Se ha generado el resumen del cobro. Responde brevemente pidiendo al usuario que lo revise y confirme, sin incluir el resumen. Aun no se ha cobrado nada. El usuario te avisará cuando el pago sea aprobado o rechazado."
        });
    }

    [DisplayName("cancel_booking")]
    [Description("Genera un resumen del proceso de cancelación de una reserva existente para que el usuario lo apruebe. Requiere can_cancel a true.")]
    public async Task<string> CancelBooking(
        [Description("ID de la reserva obtenido de get_my_bookings. Nunca un número dicho por el usuario: no conoce los IDs")] int bookingId)
    {
        GetCurrentUserBookingDetailDtoResponse booking;
        try
        {
            booking = await _mediator.Send(new GetBookingDetail(bookingId));
            var summary = BuildCancellationSummary(booking);

            ToolApprovalRequest approvalRequest = new CancelBookingApprovalRequest(
                $"{ApprovalToolNames.CancelBooking}_{Guid.NewGuid()}",
                ApprovalToolNames.CancelBooking,
                summary,
                bookingId);

            await _cacheService.SetAsync(ChatCacheKeyReference.ManualApproval(sessionId), approvalRequest, TimeSpan.FromMinutes(10));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            booking_id = bookingId,
            message = "Se ha generado el resumen de la cancelación. Responde brevemente pidiendo al usuario que lo revise y confirme, sin incluir el resumen. Aun no se ha cancelado nada. El usuario te avisará cuando la cancelación sea aprobada o rechazada."
        });
    }

    private static string BuildPaymentSummary(GetCurrentUserBookingDetailDtoResponse booking, PaymentMethodResponse paymentMethod)
    {
        var culture = CultureInfo.GetCultureInfo("es-ES");
        var sb = new StringBuilder();

        sb.AppendLine("# Resumen del cobro");
        sb.AppendLine();

        AppendBookingLines(sb, booking, culture);

        sb.AppendLine();
        sb.AppendLine("## Método de pago");
        sb.AppendLine();
        sb.AppendLine($"{paymentMethod.Card.Brand} terminada en {paymentMethod.Card.Last4} (caduca {paymentMethod.Card.ExpMonth:00}/{paymentMethod.Card.ExpYear})");

        sb.AppendLine();
        sb.AppendLine($"**Importe a cobrar: {GetBookingTotal(booking).ToString("C", culture)}**");

        return sb.ToString();
    }

    private static string BuildCancellationSummary(GetCurrentUserBookingDetailDtoResponse booking)
    {
        var culture = CultureInfo.GetCultureInfo("es-ES");
        var sb = new StringBuilder();

        sb.AppendLine("# Resumen de la cancelación");
        sb.AppendLine();

        AppendBookingLines(sb, booking, culture);

        sb.AppendLine();

        if (booking.ChargedAt.HasValue)
        {
            sb.AppendLine($"La reserva ya está cobrada, así que se emitirá un reembolso de {GetBookingTotal(booking).ToString("C", culture)} que tardará unos días en aparecer en la tarjeta.");
        }
        else
        {
            sb.AppendLine("La reserva todavía no está cobrada, así que no habrá ningún reembolso.");
        }

        sb.AppendLine();
        sb.AppendLine("**Esta operación es irreversible.**");

        return sb.ToString();
    }

    private static void AppendBookingLines(StringBuilder sb, GetCurrentUserBookingDetailDtoResponse booking, CultureInfo culture)
    {
        sb.AppendLine($"| Reserva | {booking.Id} |");
        sb.AppendLine("| --- | --- |");
        sb.AppendLine($"| Crucero | {booking.Name} |");
        sb.AppendLine($"| Destino | {booking.Destination} |");
        sb.AppendLine($"| Barco | {booking.ShipName} |");
        sb.AppendLine($"| Salida | {booking.StartDate.ToString("d", culture)} |");
        sb.AppendLine($"| Regreso | {booking.EndDate.ToString("d", culture)} |");

        sb.AppendLine();
        sb.AppendLine("## Camarotes");
        sb.AppendLine();
        sb.AppendLine("| Camarote | Nº Pasajeros | Precio |");
        sb.AppendLine("| --- | --- | --- |");

        foreach (var cabin in booking.Cabins)
        {
            sb.AppendLine($"| {cabin.TypeName} | {cabin.Occupants} | {cabin.Price.ToString("C", culture)} |");
        }

        if (booking.Extras.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Extras");
            sb.AppendLine();
            sb.AppendLine("| Extra | Precio |");
            sb.AppendLine("| --- | --- |");

            foreach (var extra in booking.Extras)
            {
                sb.AppendLine($"| {extra.Name} | {extra.Price.ToString("C", culture)} |");
            }
        }
    }

    private static decimal GetBookingTotal(GetCurrentUserBookingDetailDtoResponse booking)
        => booking.Cabins.Sum(c => c.Price) + booking.Extras.Sum(e => e.Price);
}

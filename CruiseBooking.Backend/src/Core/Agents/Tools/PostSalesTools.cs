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
using System.Text.Json;

namespace Core.Agents.Tools;

/// <summary>
/// Herramientas de gestion de reservas existentes expuestas al modelo de lenguaje.
/// </summary>
internal sealed class PostSalesTools(IMediator mediator, ICacheService cacheService, Guid sessionId)
{
    private readonly IMediator _mediator = mediator;
    private readonly ICacheService _cacheService = cacheService;

    [DisplayName(PostSalesToolNames.GetMyBookings)]
    [Description("Devuelve todas las reservas del usuario")]
    public async Task<string> GetMyBookings()
    {
        IReadOnlyList<GetCurrentUserBookingSummaryDtoResponse> bookings;
        try
        {
            bookings = await _mediator.Send(new GetBookingsSummary());
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { ok = true, error = ex.Message });
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

    [DisplayName(PostSalesToolNames.GetBookingDetail)]
    [Description("Devuelve el desglose de camarotes, extras y precios para una reserva existente")]
    public async Task<string> GetBookingDetail(
        [Description("Id de la reserva")] int bookingId)
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

    [DisplayName(PostSalesToolNames.GetPaymentMethods)]
    [Description("Devuelve los medios de pago que el usuario tiene dados de alta en su cuenta")]
    public async Task<string> GetPaymentMethods()
    {
        List<PaymentMethodResponse> paymentMethods;
        try
        {
            paymentMethods = await _mediator.Send(new GetPaymentMethods());
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { ok = false, error = ex.Message });
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

    [DisplayName(PostSalesToolNames.PayBooking)]
    [Description("Genera un resumen del proceso de pago manual de una reserva existente para que el usario lo apruebe")]
    public async Task<string> PayBooking(
        [Description("Id de la reserva")] int bookingId,
        [Description("Identificador del método de pago elegido")] string paymentMethodId)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
        {
            return JsonSerializer.Serialize(new { ok = false, error = "Falta el método de pago. Llama antes a get_payment_methods y pide al usuario que elija una de sus tarjetas." });
        }

        try
        {
            var paymentMethods = await _mediator.Send(new GetPaymentMethods());
            var paymentMethod = paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
            if (paymentMethod is null)
            {
                return JsonSerializer.Serialize(new { ok = false, error = "Ese método de pago no está dado de alta en la cuenta del usuario. Usa únicamente uno de los que devuelve get_payment_methods." });
            }

            var booking = await _mediator.Send(new GetBookingDetail(bookingId));
            var summary = BuildPaymentSummary(booking, paymentMethod);

            ToolApprovalRequest approvalRequest = new PayApprovalRequest(
                $"{PostSalesToolNames.PayBooking}_{Guid.NewGuid()}",
                PostSalesToolNames.PayBooking,
                summary,
                bookingId,
                paymentMethodId);

            await _cacheService.SetAsync(ChatCacheKeyReference.ManualApproval(sessionId), approvalRequest, TimeSpan.FromMinutes(10));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { ok = false, error = ex.Message });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            bookingId,
            message = "Se ha generado el resumen del cobro. Responde brevemente pidiendo al usuario que lo revise y confirme, sin incluir el resumen."
        });
    }

    [DisplayName(PostSalesToolNames.CancelBooking)]
    [Description("Genera un resumen del proceso de cancelación de una reserva existente para que el usuario lo apruebe")]
    public async Task<string> CancelBooking(
        [Description("Id de la reserva")] int bookingId)
    {
        GetCurrentUserBookingDetailDtoResponse booking;
        try
        {
            booking = await _mediator.Send(new GetBookingDetail(bookingId));
            var summary = BuildCancellationSummary(booking);

            ToolApprovalRequest approvalRequest = new CancelBookingApprovalRequest(
                $"{PostSalesToolNames.CancelBooking}_{Guid.NewGuid()}",
                PostSalesToolNames.CancelBooking,
                summary,
                bookingId);

            await _cacheService.SetAsync(ChatCacheKeyReference.ManualApproval(sessionId), approvalRequest, TimeSpan.FromMinutes(10));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { ok = false, error = ex.Message });
        }

        return JsonSerializer.Serialize(new
        {
            ok = true,
            bookingId,
            message = "Se ha generado el resumen de la cancelación. Responde brevemente pidiendo al usuario que lo revise y confirme, sin incluir el resumen."
        });
    }

    private static PaymentSummary BuildPaymentSummary(GetCurrentUserBookingDetailDtoResponse booking, PaymentMethodResponse paymentMethod)
    {
        var card = new SummaryPaymentMethod(
            paymentMethod.Card.Brand,
            paymentMethod.Card.Last4,
            (int)paymentMethod.Card.ExpMonth,
            (int)paymentMethod.Card.ExpYear);

        return new PaymentSummary(
            "Resumen del cobro",
            ToHeader(booking),
            ToCabins(booking),
            ToExtras(booking),
            card,
            GetBookingTotal(booking));
    }

    private static CancellationSummary BuildCancellationSummary(GetCurrentUserBookingDetailDtoResponse booking)
    {
        var alreadyCharged = booking.ChargedAt.HasValue;

        return new CancellationSummary(
            "Resumen de la cancelación",
            ToHeader(booking),
            ToCabins(booking),
            ToExtras(booking),
            alreadyCharged,
            alreadyCharged ? GetBookingTotal(booking) : 0m);
    }

    private static SummaryBookingHeader ToHeader(GetCurrentUserBookingDetailDtoResponse booking)
        => new(booking.Id, booking.Name, booking.Destination, booking.ShipName, booking.StartDate, booking.EndDate);

    private static SummaryCabinLine[] ToCabins(GetCurrentUserBookingDetailDtoResponse booking)
        => [.. booking.Cabins.Select(c => new SummaryCabinLine(c.TypeName, c.Occupants, c.Price))];

    private static SummaryExtraLine[] ToExtras(GetCurrentUserBookingDetailDtoResponse booking)
        => [.. booking.Extras.Select(e => new SummaryExtraLine(e.Name, e.Price))];

    private static decimal GetBookingTotal(GetCurrentUserBookingDetailDtoResponse booking)
        => booking.Cabins.Sum(c => c.Price) + booking.Extras.Sum(e => e.Price);
}

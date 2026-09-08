using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Cobra la reserva con el método de pago elegido cuando el usuario aprueba el pago.
/// </summary>
internal sealed class PayBookingApprovalHandler(IMediator mediator)
    : ToolApprovalHandler<PayApprovalRequest>
{
    private readonly IMediator _mediator = mediator;

    /// <inheritdoc />
    protected override async Task HandleCoreAsync(Guid sessionId, PayApprovalRequest request, bool approved, CancellationToken ct)
    {
        await _mediator.Send(new PayBooking(request.BookingId, new PayBookingRequest(request.PaymentMethodId)), ct);
    }
}

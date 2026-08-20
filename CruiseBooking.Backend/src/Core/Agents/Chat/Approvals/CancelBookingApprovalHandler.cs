using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.UseCases.Bookings;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Cancela la reserva cuando el usuario aprueba la cancelación.
/// </summary>
internal sealed class CancelBookingApprovalHandler(IMediator mediator)
    : ToolApprovalHandler<CancelBookingApprovalRequest>
{
    private readonly IMediator _mediator = mediator;

    /// <inheritdoc />
    protected override async Task HandleCoreAsync(Guid sessionId, CancelBookingApprovalRequest request, bool approved, CancellationToken ct)
    {
        await _mediator.Send(new CancelBooking(request.BookingId), ct);
    }
}

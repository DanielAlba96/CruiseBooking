using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.UseCases.Bookings;
using Shared.Domain.Exceptions;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Cancela la reserva cuando el usuario aprueba la cancelación.
/// </summary>
internal sealed class CancelBookingApprovalHandler(IMediator mediator)
    : ToolApprovalHandler<CancelBookingApprovalRequest>
{
    private readonly IMediator _mediator = mediator;

    /// <inheritdoc />
    protected override async Task<string> HandleCoreAsync(Guid sessionId, CancelBookingApprovalRequest request, bool approved, CancellationToken ct)
    {
        if (!approved)
            return $"He rechazado la ejecucion de la herramienta {request.ToolName}";

        try
        {
            await _mediator.Send(new CancelBooking(request.BookingId), ct);

            return $"He aprobado la ejecucion de la herramienta {request.ToolName}";
        }
        catch (ControlledException ex)
        {
            return $"Ha ocurrido un error al aprobar la herramienta {request.ToolName}: {ex.Message}";
        }
    }
}

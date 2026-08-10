using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
using Shared.Domain.Exceptions;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Cobra la reserva con el método de pago elegido cuando el usuario aprueba el pago.
/// </summary>
internal sealed class PayBookingApprovalHandler(IMediator mediator)
    : ToolApprovalHandler<PayApprovalRequest>
{
    private readonly IMediator _mediator = mediator;

    /// <inheritdoc />
    protected override async Task<string> HandleCoreAsync(Guid sessionId, PayApprovalRequest request, bool approved, CancellationToken ct)
    {
        if (!approved)
            return $"He rechazado la ejecucion de la herramienta {request.ToolName}";

        try
        {
            await _mediator.Send(new PayBooking(request.BookingId, new PayBookingRequest(request.PaymentMethodId)), ct);

            return $"He aprobado la ejecucion de la herramienta {request.ToolName}";
        }
        catch (ControlledException ex)
        {
            return $"Ha ocurrido un error al aprobar la herramienta {request.ToolName}: {ex.Message}";
        }
    }
}

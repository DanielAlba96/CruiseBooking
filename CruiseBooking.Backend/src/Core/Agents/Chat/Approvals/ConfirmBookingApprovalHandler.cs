using Core.Agents.Common;
using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
using Shared.Domain.Exceptions;
using Shared.Domain.Services;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Crea la reserva a partir del borrador en curso cuando el usuario aprueba su confirmación.
/// </summary>
internal sealed class ConfirmBookingApprovalHandler(ICacheService cacheService, IMediator mediator)
    : ToolApprovalHandler<ConfirmBookingApprovalRequest>
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly IMediator _mediator = mediator;

    /// <inheritdoc />
    protected override async Task<string> HandleCoreAsync(Guid sessionId, ConfirmBookingApprovalRequest request, bool approved, CancellationToken ct)
    {
        if (!approved)
            return $"He rechazado la ejecucion de la herramienta {request.ToolName}";

        try
        {
            var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId), ct);
            if (draft is null)
                return $"Ha ocurrido un error al aprobar la herramienta {request.ToolName}: El borrador ya no está disponible. Es necesario volver a realizar el proceso de reserva";

            var bookingDto = new CreateBookingRequest
            {
                CruiseDateId = draft.CruiseDateId,
                SelectedCabins = [.. draft.Cabins.Select(c => new BookingCabinSelection(c.Id, c.Occupants, c.Price))],
                SelectedExtras = [.. draft.Extras.Select(e => e.ExtraId)]
            };

            await _mediator.Send(new CompleteBooking(bookingDto), ct);

            return $"He aprobado la ejecucion de la herramienta {request.ToolName}";
        }
        catch (ControlledException ex)
        {
            return $"Ha ocurrido un error al aprobar la herramienta {request.ToolName}: {ex.Message}";
        }
    }
}

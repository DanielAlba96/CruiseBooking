using Core.Agents.Common;
using Core.Agents.Models;
using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
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
    protected override async Task HandleCoreAsync(Guid sessionId, ConfirmBookingApprovalRequest request, bool approved, CancellationToken ct)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId), ct)
            ?? throw new NotFoundException("El borrador ya no está disponible");

        var bookingDto = new CreateBookingRequest
        {
            CruiseDateId = draft.CruiseDateId,
            SelectedCabins = [.. draft.Cabins.Select(c => new BookingCabinSelection(c.Id, c.Occupants, c.Price))],
            SelectedExtras = [.. draft.Extras.Select(e => e.ExtraId)]
        };

        await _mediator.Send(new CompleteBooking(bookingDto), ct);
    }
}

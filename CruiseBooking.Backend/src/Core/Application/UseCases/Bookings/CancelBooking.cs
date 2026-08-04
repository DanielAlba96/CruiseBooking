using Core.Application.Common.Exceptions;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Bookings;

/// <summary>Cancela una reserva del usuario actual, reembolsando el cargo si ya se hubiera cobrado.</summary>
/// <param name="BookingId">Identificador de la reserva a cancelar.</param>
public sealed record CancelBooking(int BookingId) : IRequest;

/// <summary>Atiende <see cref="CancelBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="paymentService">Pasarela de pago, usada para el reembolso.</param>
/// <param name="workflowService">Servicio de workflows, usado para notificar la cancelación.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class CancelBookingHandler(
    IBookingRepository bookingRepository,
    IPaymentService paymentService,
    IWorkflowService workflowService,
    UserInfo userInfo) : IRequestHandler<CancelBooking>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IWorkflowService _workflowService = workflowService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(CancelBooking request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAndUserId(request.BookingId, _userInfo.Id)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFound, request.BookingId));

        if (booking.Status == BookingStatus.Canceled)
        {
            throw new ValidationException(ErrorMessages.BookingAlreadyCanceled);
        }

        if (string.IsNullOrWhiteSpace(booking.ChargeId))
        {
            await CancelAndNotify(booking.Id);
        }
        else
        {
            if (booking.CruiseDate!.CheckInStartDate <= DateTime.UtcNow)
            {
                throw new ValidationException(ErrorMessages.BookingCheckInAlreadyStarted);
            }

            var refundResult = await _paymentService.RefundAsync(booking.ChargeId, "requested_by_customer");
            if (!refundResult.Success)
            {
                throw new ValidationException(refundResult.ErrorMessage ?? ErrorMessages.RefundFailed);
            }

            await CancelAndNotify(booking.Id, refundResult.RefundId, DateTime.UtcNow);
        }
    }

    private async Task CancelAndNotify(int bookingId, string? refundId = null, DateTime? refundedAt = null)
    {
        var canceled = await _bookingRepository.CancelBooking(bookingId, refundId, refundedAt);
        if (!canceled)
        {
            throw new NotFoundException(string.Format(ErrorMessages.BookingNotFound, bookingId));
        }

        await _workflowService.SendBookingCanceledEvent(bookingId);
    }
}

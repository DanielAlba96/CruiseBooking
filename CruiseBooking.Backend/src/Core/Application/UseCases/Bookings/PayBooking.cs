using Core.Application.Common.Exceptions;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Bookings;

/// <summary>Realiza el pago manual de una reserva pendiente de pago del usuario actual.</summary>
/// <param name="BookingId">Identificador de la reserva a pagar.</param>
/// <param name="Payment">Método de pago con el que autorizar el cobro.</param>
public sealed record PayBooking(int BookingId, PayBookingRequest Payment) : IRequest;

/// <summary>Atiende <see cref="PayBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="paymentService">Pasarela de pago, usada para el cobro.</param>
/// <param name="workflowService">Servicio de workflows, usado para notificar el pago manual.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class PayBookingHandler(
    IBookingRepository bookingRepository,
    IPaymentService paymentService,
    IWorkflowService workflowService,
    UserInfo userInfo) : IRequestHandler<PayBooking>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IWorkflowService _workflowService = workflowService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(PayBooking request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAndUserId(request.BookingId, _userInfo.Id)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFound, request.BookingId));

        if (booking.Status == BookingStatus.Canceled)
        {
            throw new BookingPaymentNotAllowedException(ErrorMessages.BookingCanceledCannotPay);
        }

        if (booking.Status != BookingStatus.Created && booking.Status != BookingStatus.PendingPayment)
        {
            throw new BookingPaymentNotAllowedException(ErrorMessages.BookingNotPendingPayment);
        }

        if (string.IsNullOrWhiteSpace(request.Payment.PaymentMethodId))
        {
            throw new ValidationException(ErrorMessages.PaymentMethodRequired);
        }

        var customerId = _userInfo.CustomerId
            ?? throw new UnauthorizedException(ErrorMessages.PaymentCustomerMissing);

        var subtotal = booking.Cabins.Sum(x => x.Price) + booking.Extras.Sum(x => x.Price);
        var charge = await _paymentService.ChargeAsync(customerId, request.Payment.PaymentMethodId, subtotal);
        if (!charge.Success)
        {
            throw new PaymentFailedException(charge.ErrorMessage ?? ErrorMessages.PaymentFailed);
        }

        if (!await _bookingRepository.MarkBookingAsPaid(booking.Id, charge.ChargeId, request.Payment.PaymentMethodId, charge.ChargeAmount, DateTime.UtcNow))
        {
            await _paymentService.RefundAsync(charge.ChargeId, "duplicate");
            throw new BookingPaymentNotAllowedException(ErrorMessages.BookingAlreadyPaid);
        }

        await _workflowService.SendManualPaymentEvent(booking.Id);
    }
}

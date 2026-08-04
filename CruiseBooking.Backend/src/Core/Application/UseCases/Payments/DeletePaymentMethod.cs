using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Resources;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Payments;

/// <summary>Da de baja un medio de pago, siempre que no esté ligado a una reserva activa.</summary>
/// <param name="PaymentMethodId">Identificador del medio de pago en la pasarela.</param>
public sealed record DeletePaymentMethod(string PaymentMethodId) : IRequest;

/// <summary>Atiende <see cref="DeletePaymentMethod"/>.</summary>
/// <param name="paymentService">Pasarela de pago.</param>
/// <param name="bookingRepository">Repositorio de reservas.</param>
public sealed class DeletePaymentMethodHandler(
    IPaymentService paymentService,
    IBookingRepository bookingRepository) : IRequestHandler<DeletePaymentMethod>
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <inheritdoc />
    public async Task Handle(DeletePaymentMethod request, CancellationToken cancellationToken)
    {
        var exists = await _bookingRepository.ExistsActiveBookingByPaymentMethod(request.PaymentMethodId);
        if (exists)
        {
            throw new ValidationException(ErrorMessages.PaymentMethodInUse);
        }

        await _paymentService.DeletePaymentMethod(request.PaymentMethodId);
    }
}

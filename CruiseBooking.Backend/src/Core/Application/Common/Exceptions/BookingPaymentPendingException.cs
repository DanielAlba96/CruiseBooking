using Core.Application.Resources;
using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando se intenta acceder al check-in de una reserva cuyo pago aún no se ha realizado.</summary>
public sealed class BookingPaymentPendingException()
    : ControlledException(ErrorMessages.BookingPaymentPending)
{
    /// <inheritdoc />
    public override int StatusCode => 402;
}

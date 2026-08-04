using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando se intenta pagar una reserva cancelada o que ya está pagada.</summary>
public sealed class BookingPaymentNotAllowedException(string reason)
    : ControlledException(reason)
{
    /// <inheritdoc />
    public override int StatusCode => 409;
}

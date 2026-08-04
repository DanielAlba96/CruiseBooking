using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando el proveedor de pagos rechaza el cobro de una reserva.</summary>
public sealed class PaymentFailedException(string reason)
    : ControlledException(reason)
{
    /// <inheritdoc />
    public override int StatusCode => 402;
}

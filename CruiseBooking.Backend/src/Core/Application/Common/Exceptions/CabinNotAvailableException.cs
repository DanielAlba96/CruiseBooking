using Core.Application.Resources;
using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando se intenta cerrar una reserva con camarotes solicitados sin bloqueo vigente que los respalde, porque han expirado, nunca se bloquearon o no coinciden.</summary>
public sealed class CabinNotAvailableException()
    : ControlledException(ErrorMessages.CabinNotAvailable)
{
    /// <inheritdoc />
    public override int StatusCode => 409;
}

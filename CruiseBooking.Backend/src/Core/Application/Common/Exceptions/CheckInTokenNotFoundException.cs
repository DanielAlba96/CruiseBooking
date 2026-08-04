using Core.Application.Resources;
using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando el token de check-in no corresponde a ninguna reserva pendiente.</summary>
public sealed class CheckInTokenNotFoundException()
    : ControlledException(ErrorMessages.CheckInTokenNotFound)
{
    /// <inheritdoc />
    public override int StatusCode => 404;
}

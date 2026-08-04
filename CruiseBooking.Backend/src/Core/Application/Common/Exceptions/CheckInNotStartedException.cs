using Core.Application.Resources;
using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando se intenta operar sobre el check-in antes de su fecha de apertura.</summary>
/// <param name="reason">Mensaje que indica a partir de cuándo estará disponible el check-in.</param>
public sealed class CheckInNotStartedException()
    : ControlledException(ErrorMessages.CheckInNotStarted)
{
    /// <inheritdoc />
    public override int StatusCode => 409;
}

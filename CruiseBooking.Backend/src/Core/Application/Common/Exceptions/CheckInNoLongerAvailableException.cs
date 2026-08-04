using Core.Application.Resources;
using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando la reserva existe pero el check-in ya no está disponible (cancelada o crucero zarpado).</summary>
public sealed class CheckInNoLongerAvailableException()
    : ControlledException(ErrorMessages.CheckInNoLongerAvailable)
{
    /// <inheritdoc />
    public override int StatusCode => 410;
}

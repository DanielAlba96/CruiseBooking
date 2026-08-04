using Shared.Domain.Exceptions;
using Shared.Persistence.Resources;

namespace Shared.Persistence.Exceptions;

/// <summary>Se lanza cuando no quedan unidades libres del camarote solicitado en la fecha de crucero.</summary>
/// <param name="cabinId">Identificador del camarote sin disponibilidad.</param>
public sealed class InsufficientCabinAvailabilityException(int cabinId)
    : ControlledException(string.Format(ErrorMessages.InsufficientCabinAvailability, cabinId))
{
    /// <inheritdoc />
    public override int StatusCode => 409;
}

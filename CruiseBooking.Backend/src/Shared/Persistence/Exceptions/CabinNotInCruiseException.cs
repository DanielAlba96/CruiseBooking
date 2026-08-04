using Shared.Domain.Exceptions;
using Shared.Persistence.Resources;

namespace Shared.Persistence.Exceptions;

/// <summary>Se lanza cuando se intenta bloquear un camarote que no pertenece al barco del crucero indicado.</summary>
/// <param name="cabinId">Identificador del camarote que no pertenece al crucero.</param>
public sealed class CabinNotInCruiseException(int cabinId)
    : ControlledException(string.Format(ErrorMessages.CabinNotInCruise, cabinId))
{
    /// <inheritdoc />
    public override int StatusCode => 400;
}

using Shared.Domain.Exceptions;
using Shared.Persistence.Resources;

namespace Shared.Persistence.Exceptions;

/// <summary>Se lanza cuando el número de pasajeros solicitado para un camarote no cabe en su aforo.</summary>
/// <param name="cabinId">Identificador del camarote.</param>
/// <param name="occupants">Pasajeros solicitados.</param>
/// <param name="maxOccupancy">Aforo máximo del tipo de camarote.</param>
public sealed class InvalidCabinOccupancyException(int cabinId, int occupants, int maxOccupancy)
    : ControlledException(string.Format(ErrorMessages.InvalidCabinOccupancy, cabinId, occupants, maxOccupancy))
{
    /// <inheritdoc />
    public override int StatusCode => 400;
}

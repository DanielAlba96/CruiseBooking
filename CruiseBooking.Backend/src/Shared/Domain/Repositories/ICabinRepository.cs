using Shared.Domain.Entities;

namespace Shared.Domain.Repositories;

/// <summary>Repository for ship cabin queries.</summary>
public interface ICabinRepository
{
    /// <summary>Obtiene las cabinas del barco por identificador del barco.</summary>
    /// <param name="shipId">Identificador del barco.</param>
    /// <returns>Consulta de cabinas del barco.</returns>
    IQueryable<ShipCabin> GetShipCabins(int shipId);

    /// <summary>Obtiene una cabina del barco por identificador</summary>
    /// <param name="cabinId">Identificador de la cabina del barco.</param>
    /// <returns>Datos de la cabina del barco</returns>
    Task<ShipCabin?> GetCabinById(int cabinId);

    /// <summary>Obtiene las cabinas disponibles por identificador de la fecha del crucero.</summary>
    /// <param name="cruiseDateId">Identificador de la fecha del crucero.</param>
    /// <returns>Lista de cabinas para la fecha del crucero especificada.</returns>
    Task<IReadOnlyList<ShipCabin>> GetCabinsByCruiseDate(int cruiseDateId);

    /// <summary>Bloquea una única unidad de cabina para un usuario, sin afectar a los bloqueos existentes de otras cabinas.</summary>
    /// <param name="cruiseDateId">Identificador de la fecha de crucero.</param>
    /// <param name="userId">Identificador del usuario que solicita el bloqueo.</param>
    /// <param name="cabinId">Identificador de la cabina del barco.</param>
    /// <param name="price">Precio a aplicar a esta unidad de cabina.</param>
    /// <param name="occupants">Número de pasajeros que ocuparán la cabina.</param>
    /// <returns>El identificador del bloqueo (<see cref="LockedCabin"/>) creado.</returns>
    Task<int> LockCabin(int cruiseDateId, int userId, int cabinId, decimal price, int occupants);

    /// <summary>Libera una unidad de cabina bloqueada de un usuario: borra uno de los registros de bloqueo de ese CabinId.</summary>
    Task UnlockCabin(int cruiseDateId, int cabinId, int userId);

    /// <summary>Obtiene los bloqueos de cabina vigentes de un usuario para una fecha de salida, un registro por unidad de cabina.</summary>
    Task<IReadOnlyList<LockedCabin>> GetLockedCabins(int cruiseDateId, int userId);

    /// <summary>Elimina todos los bloqueos de cabina vigentes de un usuario para una fecha de crucero (tras completar una reserva).</summary>
    Task ClearLockedCabins(int cruiseDateId, int userId);

    /// <summary>Elimina todos los bloqueos de cabina vigentes con los ids indicados</summary>
    Task ClearLockedCabinsBulk(IReadOnlyList<int> lockedCabinsIds, CancellationToken cancellationToken);
}

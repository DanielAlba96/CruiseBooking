using Shared.Domain.Entities;

namespace Shared.Domain.Repositories;

public interface IShipRepository
{
    /// <summary>
    /// Obtiene un barco por su identificador.
    /// </summary>
    /// <param name="shipId">El identificador del barco.</param>
    /// <returns>Una consulta LINQ para el barco solicitado.</returns>
    IQueryable<Ship> GetShip(int shipId);

    /// <summary>
    /// Obtiene todos los barcos disponibles.
    /// </summary>
    /// <returns>Una consulta LINQ para todos los barcos.</returns>
    IQueryable<Ship> GetShips();
}

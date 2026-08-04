using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Repositories;

public class ShipRepository(IDbContextFactory<CruisesDbContext> contextFactory) : IShipRepository, IAsyncDisposable
{
    private readonly CruisesDbContext _context = contextFactory.CreateDbContext();

    /// <summary>
    /// Obtiene una nave por su identificador.
    /// </summary>
    /// <param name="shipId">Identificador de la nave.</param>
    /// <returns>Consulta de la nave filtrada por ID.</returns>
    public IQueryable<Ship> GetShip(int shipId)
    {
        return _context.Ships
            .AsNoTracking()
            .Where(s => s.Id == shipId);
    }

    /// <summary>
    /// Obtiene todas las naves disponibles.
    /// </summary>
    /// <returns>Consulta con todas las naves.</returns>
    public IQueryable<Ship> GetShips()
    {
        return _context.Ships
            .AsNoTracking();
    }

    /// <summary>
    /// Libera los recursos asignados al contexto de base de datos.
    /// </summary>
    /// <returns>Tarea que representa la operación asincrónica de liberación.</returns>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}

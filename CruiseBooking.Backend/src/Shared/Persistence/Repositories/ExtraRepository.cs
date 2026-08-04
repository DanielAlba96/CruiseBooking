using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Repositories;

/// <summary>
/// Repositorio para gestionar los extras asociados a las fechas de cruceros.
/// </summary>
/// <param name="contextFactory">Factory para crear instancias de contexto de base de datos.</param>
public class ExtraRepository(IDbContextFactory<CruisesDbContext> contextFactory) : IExtraRepository, IAsyncDisposable
{
    private readonly CruisesDbContext _context = contextFactory.CreateDbContext();

    /// <summary>
    /// Obtiene una consulta de extras para una fecha de crucero específica.
    /// </summary>
    /// <param name="cruiseDateId">ID de la fecha del crucero.</param>
    /// <returns>Consulta de extras asociados a la fecha del crucero.</returns>
    public IQueryable<CruiseDateExtra> GetExtras(int cruiseDateId)
    {
        return _context.CruiseDateExtras
            .AsNoTracking()
            .Include(x => x.Extra)
            .Where(e => e.CruiseDateId == cruiseDateId);
    }

    /// <summary>
    /// Obtiene de forma asincrónica una lista de extras para una fecha de crucero específica.
    /// </summary>
    /// <param name="cruiseDateId">ID de la fecha del crucero.</param>
    /// <returns>Lista de lectura de extras asociados a la fecha del crucero.</returns>
    public async Task<IReadOnlyList<CruiseDateExtra>> GetExtrasAsync(int cruiseDateId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.CruiseDateExtras
            .AsNoTracking()
            .Include(x => x.Extra)
            .Where(e => e.CruiseDateId == cruiseDateId)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene de forma asincrónica un extra específico para una fecha de crucero.
    /// </summary>
    /// <param name="cruiseDateId">ID de la fecha del crucero.</param>
    /// <param name="extraId">ID del extra a buscar.</param>
    /// <returns>El extra encontrado o null si no existe.</returns>
    public async Task<CruiseDateExtra?> GetCruiseDateExtraAsync(int cruiseDateId, int extraId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.CruiseDateExtras
           .AsNoTracking()
           .Include(x => x.Extra)
           .Where(e => e.CruiseDateId == cruiseDateId)
           .FirstOrDefaultAsync(e => e.ExtraId == extraId);
    }

    /// <summary>
    /// Libera de forma asincrónica los recursos del contexto de base de datos.
    /// </summary>
    /// <returns>Tarea que se completa cuando se liberan los recursos.</returns>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}

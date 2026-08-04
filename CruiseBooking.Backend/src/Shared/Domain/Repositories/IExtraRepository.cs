using Shared.Domain.Entities;

namespace Shared.Domain.Repositories;

public interface IExtraRepository
{
    /// <summary>
    /// Obtiene una consulta de extras para una fecha de crucero específica.
    /// </summary>
    /// <param name="cruiseDateId">Identificador de la fecha del crucero.</param>
    /// <returns>Consulta IQueryable de extras de la fecha del crucero.</returns>
    IQueryable<CruiseDateExtra> GetExtras(int cruiseDateId);

    /// <summary>
    /// Obtiene de forma asincrónica la lista de todos los extras para una fecha de crucero específica.
    /// </summary>
    /// <param name="cruiseDateId">Identificador de la fecha del crucero.</param>
    /// <returns>Lista de solo lectura de extras de la fecha del crucero.</returns>
    Task<IReadOnlyList<CruiseDateExtra>> GetExtrasAsync(int cruiseDateId);

    /// <summary>
    /// Obtiene de forma asincrónica un extra específico de una fecha de crucero.
    /// </summary>
    /// <param name="cruiseDateId">Identificador de la fecha del crucero.</param>
    /// <param name="extraId">Identificador del extra.</param>
    /// <returns>El extra de la fecha del crucero, o nulo si no existe.</returns>
    Task<CruiseDateExtra?> GetCruiseDateExtraAsync(int cruiseDateId, int extraId);
}

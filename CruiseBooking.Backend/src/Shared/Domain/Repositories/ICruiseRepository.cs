using Shared.Domain.Entities;

namespace Shared.Domain.Repositories;

public interface ICruiseRepository
{
    /// <summary>
    /// Obtiene las fechas disponibles para un crucero específico.
    /// </summary>
    /// <param name="cruiseId">Identificador del crucero.</param>
    /// <returns>Consulta de fechas del crucero.</returns>
    IQueryable<CruiseDate> GetCruiseDates(int cruiseId);

    /// <summary>
    /// Obtiene los cruceros destacados o recomendados.
    /// </summary>
    /// <returns>Consulta de cruceros destacados.</returns>
    IQueryable<Cruise> GetFeaturedCruises();

    /// <summary>
    /// Obtiene la lista de puertos disponibles en los cruceros.
    /// </summary>
    /// <returns>Consulta de nombres de puertos.</returns>
    IQueryable<string> GetCruisePorts();

    /// <summary>
    /// Busca cruceros aplicando filtros opcionales.
    /// </summary>
    /// <param name="zone">Zona o región geográfica del crucero.</param>
    /// <param name="minDays">Duración mínima del crucero en días.</param>
    /// <param name="maxDays">Duración máxima del crucero en días.</param>
    /// <param name="adultsOnly">Indica si se buscan cruceros solo para adultos.</param>
    /// <param name="featuredOnly">Indica si se buscan solo cruceros destacados.</param>
    /// <returns>Consulta de cruceros filtrados.</returns>
    IQueryable<Cruise> SearchCruises(string? zone = null, int? minDays = null, int? maxDays = null, bool adultsOnly = false, bool featuredOnly = false);

    /// <summary>
    /// Busca cruceros de forma asincrónica aplicando filtros opcionales.
    /// </summary>
    /// <param name="zone">Zona o región geográfica del crucero.</param>
    /// <param name="minDays">Duración mínima del crucero en días.</param>
    /// <param name="maxDays">Duración máxima del crucero en días.</param>
    /// <param name="adultsOnly">Indica si se buscan cruceros solo para adultos.</param>
    /// <param name="featuredOnly">Indica si se buscan solo cruceros destacados.</param>
    /// <param name="itemsPerPage">Número de elementos por página</param>
    /// <param name="featuredOnly">Número de página</param>
    /// <returns>Tarea con lista paginada de cruceros filtrados.</returns>
    Task<(IReadOnlyList<Cruise> items, bool hasMore)> SearchCruisesAsync(
        string? zone = null,
        int? minDays = null,
        int? maxDays = null,
        bool adultsOnly = false,
        int pageSize = 5,
        int page = 1);

    /// <summary>
    /// Obtiene de forma asincrónica una fecha de crucero específica.
    /// </summary>
    /// <param name="cruiseDateId">Identificador de la fecha del crucero.</param>
    /// <returns>Tarea con la fecha del crucero o nulo si no existe.</returns>
    Task<CruiseDate?> GetCruiseDateAsync(int cruiseDateId);

    /// <summary>
    /// Obtiene de forma asincrónica una fecha de crucero con sus servicios adicionales.
    /// </summary>
    /// <param name="cruiseDateId">Identificador de la fecha del crucero.</param>
    /// <returns>Tarea con la fecha del crucero incluyendo los extras.</returns>
    Task<CruiseDate> GetCruiseDateWithExtras(int cruiseDateId);
}

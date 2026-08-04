using Shared.Domain.Entities;
using Shared.Domain.Repositories;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Define los campos de consulta raíz para el esquema GraphQL de cruceros.
/// </summary>
public interface IQuery
{
    /// <summary>
    /// Obtiene la lista de puertos disponibles para los cruceros.
    /// </summary>
    /// <returns>Una colección de puertos.</returns>
    public object GetPorts();

    /// <summary>
    /// Obtiene la lista de barcos disponibles.
    /// </summary>
    /// <returns>Una colección de barcos.</returns>
    public object GetShips();

    /// <summary>
    /// Obtiene la lista de cruceros disponibles.
    /// </summary>
    /// <returns>Una colección de cruceros.</returns>
    public object GetCruises();

    /// <summary>
    /// Obtiene la lista de cruceros destacados.
    /// </summary>
    /// <returns>Una colección de cruceros destacados.</returns>
    public object GetFeaturedCruises();

    /// <summary>
    /// Obtiene la información de una fecha de crucero específica.
    /// </summary>
    /// <returns>La información de la fecha del crucero.</returns>
    public object GetCruiseDate();
}

/// <summary>
/// Configurador del tipo de objeto GraphQL para las consultas raíz de cruceros.
/// Define los campos, resolvedores y configuraciones de filtrado, proyección y paginación.
/// </summary>
public class QueryType : ObjectType<IQuery>
{
    /// <summary>
    /// Configura los campos de consulta con sus resolvedores y capacidades de filtrado, ordenamiento y paginación.
    /// </summary>
    /// <param name="descriptor">El descriptor del tipo de objeto GraphQL para IQuery.</param>
    protected override void Configure(IObjectTypeDescriptor<IQuery> descriptor)
    {
        descriptor
            .Field(x => x.GetPorts())
            .ResolveWith<QueryResolvers>(r => r.GetPorts(default!))
            .UseFiltering();

        descriptor
            .Field(x => x.GetShips())
            .ResolveWith<QueryResolvers>(r => r.GetShips(default!))
            .UseProjection()
            .UseFiltering()
            .UseSorting();

        descriptor
            .Field(x => x.GetCruises())
            .ResolveWith<QueryResolvers>(r => r.GetCruises(default!))
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();

        descriptor
            .Field(x => x.GetFeaturedCruises())
            .ResolveWith<QueryResolvers>(r => r.GetFeaturedCruises(default!));
    }
}

/// <summary>
/// Resolvedor de consultas GraphQL que proporciona acceso a los datos de cruceros, barcos y puertos.
/// Cada método actúa como resolvedor para un campo específico en el esquema GraphQL.
/// </summary>
public class QueryResolvers
{
    /// <summary>
    /// Obtiene los puertos disponibles para los cruceros del repositorio.
    /// </summary>
    /// <param name="repository">El repositorio de cruceros inyectado por el contenedor de DI.</param>
    /// <returns>Una secuencia consultable de nombres de puertos.</returns>
    public IQueryable<string> GetPorts(
        [Service] ICruiseRepository repository)
     => repository.GetCruisePorts();

    /// <summary>
    /// Obtiene la lista de barcos disponibles del repositorio.
    /// </summary>
    /// <param name="repository">El repositorio de barcos inyectado por el contenedor de DI.</param>
    /// <returns>Una secuencia consultable de entidades Ship.</returns>
    public IQueryable<Ship> GetShips(
        [Service] IShipRepository repository)
        => repository.GetShips();

    /// <summary>
    /// Obtiene la lista de cruceros disponibles utilizando búsqueda en el repositorio.
    /// </summary>
    /// <param name="repository">El repositorio de cruceros inyectado por el contenedor de DI.</param>
    /// <returns>Una secuencia consultable de entidades Cruise.</returns>
    public IQueryable<Cruise> GetCruises(
       [Service] ICruiseRepository repository)
       => repository.SearchCruises();

    /// <summary>
    /// Obtiene la lista de cruceros destacados (destacados = true).
    /// </summary>
    /// <param name="repository">El repositorio de cruceros inyectado por el contenedor de DI.</param>
    /// <returns>Una secuencia consultable de entidades Cruise que están marcadas como destacadas.</returns>
    public IQueryable<Cruise> GetFeaturedCruises(
       [Service] ICruiseRepository repository)
       => repository.SearchCruises(featuredOnly: true);
}

using Shared.Domain.Entities;
using HotChocolate.Resolvers;
using Shared.Domain.Repositories;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Define la configuración del tipo GraphQL para la entidad CruiseDate.
/// Mapea los campos del modelo de dominio a los campos expuestos en el esquema GraphQL,
/// incluyendo las relaciones con barcos y servicios adicionales.
/// </summary>
public class CruiseDateType : ObjectType<CruiseDate>
{
    /// <summary>
    /// Configura los campos y resolvedores del tipo CruiseDate en el esquema GraphQL.
    /// Establece la proyección, filtrado y ordenamiento de los campos relacionados.
    /// </summary>
    /// <param name="descriptor">Descriptor del tipo ObjectType para configurar los campos de CruiseDate.</param>
    protected override void Configure(IObjectTypeDescriptor<CruiseDate> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).IsProjected(true);
        descriptor.Field(x => x.ShipId).IsProjected(true);
        descriptor.Field(x => x.StartDate);
        descriptor.Field(x => x.PriceModifier).IsProjected(true);

        descriptor
            .Field("ship")
            .ResolveWith<CruiseDateResolvers>(r => r.GetShip(default!, default!, default!))
            .UseProjection()
            .UseFiltering()
            .UseSorting()
            .UseFirstOrDefault();

        descriptor
            .Field("extras")
            .ResolveWith<CruiseDateResolvers>(r => r.GetExtras(default!, default!))
            .UseFiltering()
            .UseSorting();
    }
}

/// <summary>
/// Proporciona los resolvedores GraphQL para los campos relacionales del tipo CruiseDate.
/// Maneja la resolución de relaciones como el barco asociado y los servicios adicionales.
/// </summary>
public class CruiseDateResolvers
{
    /// <summary>
    /// Resuelve el barco asociado a una fecha de crucero.
    /// Establece el modificador de precio y el identificador de la fecha del crucero en el contexto de resolución.
    /// </summary>
    /// <param name="cruiseDate">La entidad padre CruiseDate que contiene el identificador del barco.</param>
    /// <param name="repository">Repositorio de barcos utilizado para recuperar la entidad Ship.</param>
    /// <param name="context">Contexto del resolvedor GraphQL para gestionar estado entre resoluciones.</param>
    /// <returns>Una consulta IQueryable de barcos asociados a la fecha del crucero.</returns>
    public IQueryable<Ship> GetShip(
        [Parent] CruiseDate cruiseDate,
        [Service] IShipRepository repository,
        IResolverContext context)
    {
        context.SetScopedState("priceModifier", cruiseDate.PriceModifier);
        context.SetScopedState("cruiseDateId", cruiseDate.Id);
        return repository.GetShip(cruiseDate.ShipId);
    }

    /// <summary>
    /// Resuelve los servicios adicionales disponibles para una fecha de crucero.
    /// </summary>
    /// <param name="cruiseDate">La entidad padre CruiseDate que contiene el identificador de fecha del crucero.</param>
    /// <param name="repository">Repositorio de servicios adicionales utilizado para recuperar las entidades CruiseDateExtra.</param>
    /// <returns>Una consulta IQueryable de servicios adicionales asociados a la fecha del crucero.</returns>
    public IQueryable<CruiseDateExtra> GetExtras(
        [Parent] CruiseDate cruiseDate,
        [Service] IExtraRepository repository)
        => repository.GetExtras(cruiseDate.Id);
}

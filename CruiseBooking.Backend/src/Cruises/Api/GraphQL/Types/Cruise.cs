using Shared.Domain.Entities;
using Shared.Domain.Repositories;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Tipo de objeto GraphQL para representar un crucero con sus campos y resoluciones personalizadas.
/// </summary>
public class CruiseType : ObjectType<Cruise>
{
    /// <summary>
    /// Configura los campos y resoluciones GraphQL del tipo Crucero.
    /// Define los campos explícitos a exponer en la consulta GraphQL y configura
    /// el resolver personalizado para obtener las fechas disponibles del crucero.
    /// </summary>
    /// <param name="descriptor">Descriptor del tipo de objeto para configurar los campos del crucero.</param>
    protected override void Configure(IObjectTypeDescriptor<Cruise> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).IsProjected(true);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Zone);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.OriginPort);
        descriptor.Field(x => x.Itinerary);
        descriptor.Field(x => x.DurationInDays);
        descriptor.Field(x => x.AdultsOnly);
        descriptor.Field(x => x.Featured);

        descriptor
            .Field("dates")
            .ResolveWith<CruiseResolvers>(r => r.GetCruiseDates(default!, default!))
            .UseProjection()
            .UseFiltering()
            .UseSorting();
    }
}

/// <summary>
/// Resolvers personalizados para tipos de crucero en consultas GraphQL.
/// Proporciona lógica de resolución para campos complejos que requieren acceso al repositorio.
/// </summary>
public class CruiseResolvers
{
    /// <summary>
    /// Obtiene las fechas disponibles asociadas a un crucero específico.
    /// </summary>
    /// <param name="cruise">El objeto crucero principal del cual se obtienen las fechas.</param>
    /// <param name="repository">Repositorio de cruceros utilizado para acceder a las fechas.</param>
    /// <returns>Una consulta IQueryable de fechas de crucero que pueden ser filtradas y ordenadas.</returns>
    public IQueryable<CruiseDate> GetCruiseDates(
        [Parent] Cruise cruise,
        [Service] ICruiseRepository repository)
        => repository.GetCruiseDates(cruise.Id);
}

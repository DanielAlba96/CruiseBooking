using Shared.Domain.Entities;
using HotChocolate.Data.Filters;

namespace Cruises.Api.GraphQL.Filters;

/// <summary>
/// Define el tipo de filtro de entrada de GraphQL para las entidades de cruceros.
/// Permite filtrar cruceros por sus propiedades como id, nombre, código, zona, descripción, puerto de origen, duración, restricciones de edad y fechas asociadas.
/// </summary>
public class CruiseFilterType : FilterInputType<Cruise>
{
    protected override void Configure(IFilterInputTypeDescriptor<Cruise> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Zone);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.OriginPort);
        descriptor.Field(x => x.DurationInDays);
        descriptor.Field(x => x.AdultsOnly);
        descriptor.Field(x => x.Featured);
        descriptor.Field(x => x.CruiseDates).Name("dates").Type<ListFilterInputType<CruiseDateFilterType>>();
    }
}

using Shared.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace Cruises.Api.GraphQL.Sorts;

/// <summary>
/// Tipo de entrada de ordenamiento para consultas GraphQL de cruceros.
/// Define los campos por los cuales se pueden ordenar los cruceros en la API GraphQL.
/// </summary>
public class CruiseSortType : SortInputType<Cruise>
{
    protected override void Configure(ISortInputTypeDescriptor<Cruise> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Zone);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.OriginPort);
        descriptor.Field(x => x.DurationInDays);
        descriptor.Field(x => x.AdultsOnly);
        descriptor.Field(x => x.Featured);
    }
}

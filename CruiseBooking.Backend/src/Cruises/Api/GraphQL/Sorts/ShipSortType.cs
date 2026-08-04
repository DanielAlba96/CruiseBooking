using Shared.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace Cruises.Api.GraphQL.Sorts;

/// <summary>
/// Tipo de ordenamiento para la entidad Barco en GraphQL.
/// </summary>
public class ShipSortType : SortInputType<Ship>
{
    protected override void Configure(ISortInputTypeDescriptor<Ship> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.Company);
    }
}

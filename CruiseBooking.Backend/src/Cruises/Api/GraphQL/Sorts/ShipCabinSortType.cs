using Shared.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace Cruises.Api.GraphQL.Sorts;

/// <summary>
/// Define el tipo de entrada de ordenamiento para las cabinas de barcos en consultas GraphQL.
/// Permite ordenar cabinas por precio y tipo de cabina.
/// </summary>
public class ShipCabinSortType : SortInputType<ShipCabin>
{
    protected override void Configure(ISortInputTypeDescriptor<ShipCabin> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Price);
        descriptor.Field(x => x.CabinType!.Name).Name("name");
    }
}

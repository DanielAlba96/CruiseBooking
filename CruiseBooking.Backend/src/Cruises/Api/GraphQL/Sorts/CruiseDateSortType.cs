using Shared.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace Cruises.Api.GraphQL.Sorts;

/// <summary>
/// Tipo de entrada de ordenamiento para fechas de cruceros. Permite ordenar resultados de consultas de datos de cruceros.
/// </summary>
public class CruiseDateSortType : SortInputType<CruiseDate>
{
    protected override void Configure(ISortInputTypeDescriptor<CruiseDate> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.StartDate);
    }
}

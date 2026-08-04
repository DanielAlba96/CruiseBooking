using Shared.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace Cruises.Api.GraphQL.Sorts;

/// <summary>
/// Tipo de entrada para ordenar CruiseDateExtra en consultas de GraphQL.
/// </summary>
public class CruiseDateExtraSortType : SortInputType<CruiseDateExtra>
{
    protected override void Configure(ISortInputTypeDescriptor<CruiseDateExtra> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Price);
        descriptor.Field(x => x.Extra!.Name).Name("name");
    }
}

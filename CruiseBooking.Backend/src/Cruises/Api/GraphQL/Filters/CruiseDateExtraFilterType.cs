using Shared.Domain.Entities;
using HotChocolate.Data.Filters;

namespace Cruises.Api.GraphQL.Filters;

/// <summary>
/// Tipo de filtro para <see cref="CruiseDateExtra"/> que define los campos filtrables en GraphQL.
/// Configura filtros explícitos para precio y datos del extra asociado.
/// </summary>
public class CruiseDateExtraFilterType : FilterInputType<CruiseDateExtra>
{
    protected override void Configure(IFilterInputTypeDescriptor<CruiseDateExtra> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Price);
        descriptor.Field(x => x.Extra!.Name).Name("name");
        descriptor.Field(x => x.Extra!.Code).Name("code");
    }
}

using Shared.Domain.Entities;
using HotChocolate.Data.Filters;

namespace Cruises.Api.GraphQL.Filters;

/// <summary>
/// Tipo de filtro de entrada GraphQL para las fechas de un crucero.
/// Define los campos explícitos que pueden ser filtrados en consultas GraphQL relacionadas con fechas de crucero.
/// </summary>
public class CruiseDateFilterType : FilterInputType<CruiseDate>
{
    protected override void Configure(IFilterInputTypeDescriptor<CruiseDate> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.StartDate);
    }
}

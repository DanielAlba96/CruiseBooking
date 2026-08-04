using Shared.Domain.Entities;
using HotChocolate.Data.Filters;

namespace Cruises.Api.GraphQL.Filters;

/// <summary>
/// Tipo de filtro de entrada para consultas de barcos en GraphQL.
/// Define los campos filtrables del barco: código, nombre, descripción y compañía.
/// </summary>
public class ShipFilterType : FilterInputType<Ship>
{
    protected override void Configure(IFilterInputTypeDescriptor<Ship> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.Company);
    }
}

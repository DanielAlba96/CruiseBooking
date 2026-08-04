using Shared.Persistence;
using Cruises.Api.GraphQL.Types;

namespace Cruises.Api;

/// <summary>Extensiones personalizadas para la configuración de la API de cruceros.</summary>
public static class CustomExtensions
{
    /// <summary>Configura el servidor GraphQL de HotChocolate: tipo de consulta, filtrado/ordenamiento/proyecciones, límites de paginación y diagnósticos.</summary>
    /// <param name="builder">El constructor de aplicación web.</param>
    /// <returns>El mismo constructor, para encadenamiento.</returns>
    public static WebApplicationBuilder AddCruiseGraphQL(this WebApplicationBuilder builder)
    {
        builder
            .AddGraphQL()
            .AddQueryType<QueryType>()
            .AddTypes()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .ModifyCostOptions(o =>
             {
                 o.EnforceCostLimits = false;
             })
            .ModifyPagingOptions(opt =>
            {
                opt.MaxPageSize = 100;
                opt.DefaultPageSize = 20;
                opt.IncludeTotalCount = true;
            })
            .RegisterDbContextFactory<CruisesDbContext>()
            .AddDiagnosticEventListener<GraphqlLogger>();

        return builder;
    }
}

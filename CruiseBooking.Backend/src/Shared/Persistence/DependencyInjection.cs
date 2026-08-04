using Shared.Persistence.Repositories;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Reflection;

namespace Shared.Persistence;

/// <summary>
/// Registers the Data layer's own services in the dependency injection container: the EF Core database context factory and repository implementations.
/// </summary>
public static class DataServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Data layer services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IHostApplicationBuilder AddDataServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDataSource("CruisesDb");
        builder.Services.AddDbContextFactory<CruisesDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>(),
                sql => sql.MigrationsAssembly(Assembly.GetAssembly(typeof(CruisesDbContext))!)
            ));

        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<ICruiseRepository, CruiseRepository>();
        builder.Services.AddScoped<IShipRepository, ShipRepository>();
        builder.Services.AddScoped<ICabinRepository, CabinRepository>();
        builder.Services.AddScoped<IExtraRepository, ExtraRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        return builder;
    }
}

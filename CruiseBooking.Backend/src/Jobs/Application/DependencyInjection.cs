using Jobs.Application.Jobs;
using Jobs.Application.Jobs.Resolvers;
using Jobs.Application.Services;
using Jobs.Application.Services.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Application;

/// <summary>
/// Registra los servicios de la capa de aplicación de jobs en el contenedor de dependencias.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los jobs (casos de uso) de la capa de aplicación.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    /// <returns>La misma colección de servicios, para encadenar.</returns>
    public static IServiceCollection AddJobsApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IJobResolver, JobResolver>();
        services.AddKeyedScoped<IJob, SendEmailJob>(SendEmailJob.NAME);
        services.AddKeyedScoped<IJob, GenerateInvoiceJob>(GenerateInvoiceJob.NAME);
        services.AddKeyedScoped<IJob, LockCabinsCleanupJob>(LockCabinsCleanupJob.NAME);
        return services;
    }
}

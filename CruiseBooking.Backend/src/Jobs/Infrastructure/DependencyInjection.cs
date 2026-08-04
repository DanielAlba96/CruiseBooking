using Jobs.Infrastructure.Options;
using Jobs.Infrastructure.Services;
using Shared.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Infrastructure;

/// <summary>
/// Registra los servicios de la capa de infraestructura de jobs (envío SMTP y generación de PDF).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra las opciones e implementaciones de infraestructura de la capa de jobs.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    /// <param name="configuration">La configuración de la aplicación.</param>
    /// <returns>La misma colección de servicios, para encadenar.</returns>
    public static IServiceCollection AddJobsInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}

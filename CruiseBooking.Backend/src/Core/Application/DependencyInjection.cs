using Core.Application.Common;
using Core.Application.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application;

/// <summary>
/// Registra los servicios propios de la capa de Aplicación en el contenedor de inyección de dependencias.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa de Aplicación: las implementaciones de los casos de uso y el estado scoped que consumen.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    /// <returns>La misma colección de servicios, para poder encadenar llamadas.</returns>
    public static IServiceCollection AddCoreApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<UserInfo>();

        services.AddCustomMediator();

        return services;
    }
}

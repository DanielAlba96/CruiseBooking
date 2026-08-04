using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Application.Jobs;

/// <summary>
/// Implementación de <see cref="IJobResolver"/> que localiza el <see cref="IJob"/> por su nombre
/// usando servicios con clave (keyed services) del contenedor de dependencias.
/// </summary>
/// <param name="serviceProvider">Proveedor de servicios usado para resolver el job con clave.</param>
public sealed class JobResolver(IServiceProvider serviceProvider) : IJobResolver
{
    readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public IJob? Resolve(string jobName)
        => _serviceProvider.GetKeyedService<IJob>(jobName);
}

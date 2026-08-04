namespace Jobs.Application.Jobs;

/// <summary>
/// Resuelve la estrategia <see cref="IJob"/> que debe ejecutar un job programado a partir de su nombre.
/// </summary>
public interface IJobResolver
{
    /// <summary>
    /// Obtiene el <see cref="IJob"/> registrado con el nombre indicado.
    /// </summary>
    /// <param name="jobName">Nombre único del job tal como se registró en el planificador de Dapr.</param>
    /// <returns>El job correspondiente, o <c>null</c> si no hay ninguno registrado con ese nombre.</returns>
    IJob? Resolve(string jobName);
}

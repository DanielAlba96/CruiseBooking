namespace Jobs.Application.Jobs;

/// <summary>
/// Define la interfaz para trabajos que se pueden ejecutar de manera asincrónica.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Ejecuta el trabajo de manera asincrónica.
    /// </summary>
    /// <param name="payload">Los datos de carga útil del trabajo en formato de solo lectura.</param>
    /// <param name="cancellationToken">Token de cancelación para interrumpir la ejecución del trabajo.</param>
    /// <returns>Una tarea que representa la ejecución asincrónica del trabajo.</returns>
    Task ExecuteAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}

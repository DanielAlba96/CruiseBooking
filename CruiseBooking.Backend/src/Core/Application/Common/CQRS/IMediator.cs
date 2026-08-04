namespace Core.Application.Common.CQRS;

/// <summary>
/// Mediador CQRS que facilita el envío de solicitudes y la ejecución de handlers correspondientes.
/// </summary>
/// <remarks>
/// Implementa el patrón Mediator para desacoplar la lógica de envío de solicitudes de su procesamiento,
/// permitiendo que las solicitudes sean procesadas por handlers registrados en el contenedor de inyección de dependencias.
/// </remarks>
public interface IMediator
{
    /// <summary>
    /// Envía una solicitud que retorna una respuesta de un tipo específico.
    /// </summary>
    /// <typeparam name="TResponse">El tipo de la respuesta que retorna la solicitud.</typeparam>
    /// <param name="request">La solicitud a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación para interrumpir la operación de forma asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica y contiene la respuesta tipada.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si se solicita la cancelación antes de que se complete la operación.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una solicitud que no retorna una respuesta.
    /// </summary>
    /// <param name="request">La solicitud a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación para interrumpir la operación de forma asincrónica.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    /// <exception cref="OperationCanceledException">Se lanza si se solicita la cancelación antes de que se complete la operación.</exception>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}

namespace Core.Application.Common.CQRS;

/// <summary>
/// Define un contrato para manejar solicitudes que producen una respuesta tipada.
/// </summary>
/// <typeparam name="TRequest">El tipo de solicitud a manejar, que debe implementar <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">El tipo de respuesta que devuelve el manejador.</typeparam>
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Ejecuta el manejador de la solicitud y devuelve una respuesta tipada.
    /// </summary>
    /// <param name="command">La solicitud a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación para interrumpir la operación de forma asincrónica. Por defecto es <see cref="CancellationToken.None"/>.</param>
    /// <returns>Una tarea que representa la operación asincrónica, cuyo resultado es la respuesta del tipo <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Define un contrato para manejar solicitudes que no producen una respuesta tipada (operaciones de efecto secundario).
/// </summary>
/// <typeparam name="TRequest">El tipo de solicitud a manejar, que debe implementar <see cref="IRequest"/>.</typeparam>
public interface IRequestHandler<TRequest> where TRequest : IRequest
{
    /// <summary>
    /// Ejecuta el manejador de la solicitud sin devolver una respuesta tipada.
    /// </summary>
    /// <param name="command">La solicitud a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación para interrumpir la operación de forma asincrónica. Por defecto es <see cref="CancellationToken.None"/>.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    Task Handle(TRequest command, CancellationToken cancellationToken = default);
}

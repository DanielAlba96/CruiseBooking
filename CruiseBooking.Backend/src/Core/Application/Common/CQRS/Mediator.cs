using Core.Application.Resources;

namespace Core.Application.Common.CQRS;

/// <summary>
/// Implementación del patrón Mediador CQRS que enruta solicitudes a sus correspondientes manejadores.
/// </summary>
/// <remarks>
/// Esta clase utiliza inyección de dependencias para resolver los manejadores de solicitudes
/// del contenedor de servicios en tiempo de ejecución.
/// </remarks>
public class Mediator(IServiceProvider provider) : IMediator
{
    readonly IServiceProvider _provider = provider;

    /// <summary>
    /// Envía una solicitud con respuesta al manejador correspondiente de manera asíncrona.
    /// </summary>
    /// <typeparam name="TResponse">El tipo de respuesta esperada de la solicitud.</typeparam>
    /// <param name="request">La solicitud a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona y contiene la respuesta de tipo <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando no existe un manejador registrado para el tipo de solicitud.</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = _provider.GetService(handlerType)
            ?? throw new InvalidOperationException(string.Format(ErrorMessages.NoHandlerRegistered, request.GetType().Name));

        return await handler.Handle((dynamic)request, cancellationToken);
    }

    /// <summary>
    /// Envía una solicitud sin respuesta al manejador correspondiente de manera asíncrona.
    /// </summary>
    /// <param name="request">La solicitud a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="InvalidOperationException">Se lanza cuando no existe un manejador registrado para el tipo de solicitud.</exception>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        dynamic handler = _provider.GetService(handlerType)
            ?? throw new InvalidOperationException(string.Format(ErrorMessages.NoHandlerRegistered, request.GetType().Name));

        await handler.Handle((dynamic)request, cancellationToken);
    }
}

namespace Core.Application.Common.CQRS;

/// <summary>
/// Interfaz marcadora para solicitudes CQRS que retornan una respuesta.
/// </summary>
/// <typeparam name="TResponse">El tipo de respuesta que retorna la solicitud.</typeparam>
public interface IRequest<TResponse>;

/// <summary>
/// Interfaz marcadora para solicitudes CQRS que no retornan una respuesta.
/// </summary>
public interface IRequest;

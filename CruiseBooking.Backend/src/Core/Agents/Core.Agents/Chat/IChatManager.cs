using Core.Agents.Models;

namespace Core.Agents.Chat;

/// <summary>
/// Gestor de chat para guiar la reserva de cruceros de forma conversacional.
/// Mantiene sesiones en caché, construye herramientas de búsqueda y manejo de borradores,
/// y orquesta la interacción con el modelo de lenguaje.
/// </summary>
public interface IChatManager
{
    /// <summary>
    /// Inicia un flujo de chat asincrónico que transmite respuestas como una secuencia de strings.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="message">Mensaje del usuario a procesar en la sesión.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación asincrónica.</param>
    /// <returns>Una secuencia asincrónica de eventos que representa la respuesta del chat transmitida en tiempo real.</returns>
    IAsyncEnumerable<ChatStreamEvent> StartChatStreamAsync(Guid sessionId, string message, CancellationToken ct = default);

    /// <summary>
    /// Continua una conversacion que se detuvo para solicitar una aprobación
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="toolCallId">El Id de la ejecucion para la que se solicitó aprobación</param>
    /// <param name="approved">Resultado de la aprobación</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación asincrónica.</param>
    /// <returns>Una secuencia asincrónica de eventos que representa la respuesta del chat transmitida en tiempo real.</returns>
    IAsyncEnumerable<ChatStreamEvent> StartChatStreamWithApprovalAsync(
        Guid sessionId,
        string toolCallId,
        bool approved,
        CancellationToken ct = default);
}

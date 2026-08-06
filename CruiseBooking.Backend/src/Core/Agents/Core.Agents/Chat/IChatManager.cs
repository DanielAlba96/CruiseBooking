namespace Core.Agents.Chat;

/// <summary>
/// Define el contrato para gestionar sesiones de chat con streaming de mensajes.
/// </summary>
public interface IChatManager
{
    /// <summary>
    /// Inicia un flujo de chat asincrónico que transmite respuestas como una secuencia de strings.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="message">Mensaje del usuario a procesar en la sesión.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación asincrónica.</param>
    /// <returns>Una secuencia asincrónica de strings que representa la respuesta del chat transmitida en tiempo real.</returns>
    IAsyncEnumerable<string> StartChatStreamAsync(Guid sessionId, string message, CancellationToken ct = default);
}

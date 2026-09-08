namespace Core.Application.Models;

/// <summary>
/// Modelo de transferencia de datos para mensajes de chat.
/// </summary>
public class ChatMessageDto
{
    /// <summary>
    /// Obtiene o establece el contenido del mensaje de chat.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

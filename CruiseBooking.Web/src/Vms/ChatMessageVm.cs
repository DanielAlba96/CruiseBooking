namespace CruiseBooking.Vms;

/// <summary>
/// Representa un mensaje en una conversación de chat.
/// </summary>
public class ChatMessageVm
{
    /// <summary>
    /// Obtiene o establece un valor que indica si el mensaje fue enviado por el usuario.
    /// </summary>
    public bool IsUser { get; set; }

    /// <summary>
    /// Obtiene o establece el contenido de texto del mensaje.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece un valor que indica si el mensaje se está transmitiendo actualmente.
    /// </summary>
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si el mensaje representa un error de la conversación.
    /// </summary>
    public bool IsError { get; set; }
}

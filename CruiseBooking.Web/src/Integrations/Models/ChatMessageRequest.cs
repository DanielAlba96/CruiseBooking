using System.Text.Json.Serialization;

namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa una solicitud de mensaje de chat enviada por un usuario.
/// </summary>
/// <param name="Message">El contenido de texto del mensaje de chat.</param>
public record ChatMessageRequest(
    [property: JsonPropertyName("message")] string Message);

namespace Shared.Domain.Models;

/// <summary>
/// Representa los datos de un correo electrónico a enviar.
/// </summary>
public sealed class EmailData
{
    /// <summary>
    /// Obtiene la dirección de correo electrónico del destinatario.
    /// </summary>
    /// <value>
    /// La dirección de correo electrónico a la que se enviará el mensaje.
    /// </value>
    public string To { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el asunto del correo electrónico.
    /// </summary>
    /// <value>
    /// El asunto o línea de título del mensaje de correo.
    /// </value>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el cuerpo del correo electrónico en formato HTML.
    /// </summary>
    /// <value>
    /// El contenido HTML del mensaje de correo.
    /// </value>
    public string HtmlBody { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene la colección de archivos adjuntos del correo electrónico.
    /// </summary>
    /// <value>
    /// Una colección de solo lectura de adjuntos asociados al mensaje.
    /// </value>
    public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = [];
}

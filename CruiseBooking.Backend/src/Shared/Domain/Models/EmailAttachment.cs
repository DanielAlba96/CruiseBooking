namespace Shared.Domain.Models;

/// <summary>
/// Representa un archivo adjunto en un correo electrónico.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// Obtiene el nombre del archivo adjunto.
    /// </summary>
    /// <value>El nombre del archivo.</value>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Obtiene el tipo MIME (Content-Type) del archivo adjunto.
    /// </summary>
    /// <value>El tipo de contenido del archivo. Por defecto, "application/octet-stream".</value>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// Obtiene el contenido binario del archivo adjunto.
    /// </summary>
    /// <value>Array de bytes que contiene los datos del archivo.</value>
    public byte[] Content { get; init; } = [];
}

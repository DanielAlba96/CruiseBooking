using Shared.Domain.Models;

namespace Shared.Domain.Services;

/// <summary>
/// Servicio de envío de correos electrónicos usado por las activities del workflow.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía el mensaje de correo indicado.
    /// </summary>
    /// <param name="message">Mensaje a enviar, incluyendo destinatario, cuerpo HTML y adjuntos.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    Task SendAsync(EmailData message, CancellationToken cancellationToken);
}

using Shared.Domain.Models;
using Shared.Domain.Services;
using Microsoft.Extensions.Logging;
using Jobs.Application.Common.Exceptions;
using System.Text.Json;

namespace Jobs.Application.Jobs.Resolvers;

/// <summary>
/// Envía por SMTP un correo recibido por pub/sub, mapeando el resultado al estado de suscripción de Dapr.
/// </summary>
/// <param name="emailSender">Servicio de envío de correo.</param>
/// <param name="logger">Logger del job.</param>
public sealed class SendEmailJob(IEmailService emailSender, ILogger<SendEmailJob> logger) : IJob
{
    readonly IEmailService _emailSender = emailSender;
    readonly ILogger<SendEmailJob> _logger = logger;

    public const string NAME = "send-email";

    /// <summary>
    /// Procesa una solicitud de envío de correo.
    /// </summary>
    /// <param name="message">Mensaje a enviar; puede ser null si el cuerpo no era válido.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    public async Task ExecuteAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        EmailData? message;
        try
        {
            message = JsonSerializer.Deserialize<EmailData>(payload.Span);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Dropping email request with an invalid body.");
            return;
        }

        if (message is null || string.IsNullOrWhiteSpace(message.To))
        {
            _logger.LogWarning("Dropping email request without a recipient.");
            return;
        }

        try
        {
            await _emailSender.SendAsync(message, cancellationToken);
        }
        catch (PermanentEmailException ex)
        {
            _logger.LogWarning(ex, "Dropping email to {Recipient}: permanent failure.", message.To);
        }
    }
}

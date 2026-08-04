using System.Data.Common;
using Jobs.Infrastructure.Options;
using Shared.Domain.Models;
using Shared.Domain.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Jobs.Application.Common.Exceptions;
using Jobs.Infrastructure.Resources;

namespace Jobs.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IEmailService"/> sobre SMTP (Mailpit en desarrollo) usando MailKit.
/// </summary>
public sealed class SmtpEmailService(IConfiguration config, IOptions<EmailOptions> options) : IEmailService
{
    readonly string? _connectionString = config.GetConnectionString("mailpit");
    readonly EmailOptions _options = options.Value;

    /// <inheritdoc />
    public async Task SendAsync(EmailData message, CancellationToken cancellationToken)
    {
        var (host, port) = GetSmtpEndpoint();

        MimeMessage email;
        try
        {
            email = new MimeMessage();
            email.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            email.To.Add(MailboxAddress.Parse(message.To));
        }
        catch (ParseException ex)
        {
            throw new PermanentEmailException(string.Format(ErrorMessages.InvalidEmailAddress, message.To), ex);
        }

        email.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody
        };

        foreach (var attachment in message.Attachments)
        {
            bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        email.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.None, cancellationToken);
            await smtpClient.SendAsync(email, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);
        }
        catch (SmtpCommandException ex) when (ex.StatusCode is SmtpStatusCode.MailboxNameNotAllowed or SmtpStatusCode.MailboxUnavailable)
        {
            throw new PermanentEmailException(string.Format(ErrorMessages.SmtpRecipientRejected, message.To), ex);
        }
    }

    private (string Host, int Port) GetSmtpEndpoint()
    {
        if (_connectionString is null)
            throw new InvalidOperationException(ErrorMessages.MailPitConnectionStringMissing);

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = _connectionString
        };

        if (!builder.TryGetValue("endpoint", out var endpointValue) || endpointValue is not string endpoint)
        {
            throw new InvalidOperationException(ErrorMessages.MailPitEndpointMissing);
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var smtpUri))
        {
            throw new InvalidOperationException(ErrorMessages.MailPitEndpointInvalid);
        }

        return (smtpUri.Host, smtpUri.Port);
    }
}

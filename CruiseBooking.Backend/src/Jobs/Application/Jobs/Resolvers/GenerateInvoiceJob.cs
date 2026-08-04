using Dapr.Jobs;
using Dapr.Jobs.Models;
using Jobs.Application.Services;
using Microsoft.Extensions.Logging;
using Shared.Domain.Models;
using Shared.Domain.Repositories;
using System.Text.Json;

namespace Jobs.Application.Jobs.Resolvers;

/// <summary>
/// Genera la factura PDF de una reserva y encadena su envío invocando el job de email en el mismo proceso.
/// </summary>
/// <param name="bookingRepository">Repositorio para obtener los datos de facturación.</param>
/// <param name="invoiceService">Generador del PDF de la factura.</param>
/// <param name="daprJobService">Cliente de dapr para encolar trabajos</param>
/// <param name="logger">Logger del job.</param>
public sealed class GenerateInvoiceJob(
    IBookingRepository bookingRepository,
    IInvoiceService invoiceService,
    DaprJobsClient daprJobService,
    ILogger<GenerateInvoiceJob> logger) : IJob
{
    public const string NAME = "generate-invoice";

    /// <summary>
    /// Procesa una solicitud de factura: obtiene los datos, genera el PDF y envía el correo con el adjunto.
    /// </summary>
    /// <param name="bookingId">Identificador de la reserva a facturar.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición.</param>
    public async Task ExecuteAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        int bookingId;
        try
        {
            bookingId = JsonSerializer.Deserialize<InvoiceJobData>(payload.Span)!.BookingId;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Dropping invoice request with an invalid body.");
            return;
        }

        if (bookingId <= 0)
        {
            logger.LogWarning("Dropping invoice request without a valid booking id.");
            return;
        }

        var booking = await bookingRepository.GetBookingInvoiceData(bookingId);
        if (booking is null)
        {
            logger.LogWarning("Dropping invoice request for missing booking {BookingId}.", bookingId);
            return;
        }

        if (string.IsNullOrWhiteSpace(booking.UserEmail))
        {
            logger.LogWarning("Dropping invoice request for booking {BookingId}: no user email.", bookingId);
            return;
        }

        var invoiceFileName = $"invoice-{booking.Id}.pdf";
        byte[] invoiceBytes = await GetOrCreateInvoiceAsync(booking, invoiceFileName, cancellationToken);

        var startDate = booking.StartDate.ToString("dd/MM/yyyy");
        var checkInStartDate = booking.CheckInStartDate.ToString("dd/MM/yyyy");

        var email = new EmailData
        {
            To = booking.UserEmail,
            Subject = $"Factura de tu reserva #{booking.Id}",
            HtmlBody = $"""
                <p>Hola {booking.UserName},</p>
                <p>El pago ha sido realizado correctamente y te adjuntamos la factura de tu reserva <strong>#{booking.Id}</strong>.</p>
                <p>Crucero: {booking.CruiseName}<br/>Salida: {startDate}</p>
                <p>El {checkInStartDate} recibiras un correo con un enlace para realizar el checkin de forma online.</p>
                <p>Gracias por confiar en Cruise Booking.</p>
                """,
            Attachments =
            [
                new EmailAttachment
                {
                    FileName = invoiceFileName,
                    ContentType = "application/pdf",
                    Content = invoiceBytes
                }
            ]
        };

        await daprJobService.ScheduleJobAsync(
            $"{SendEmailJob.NAME}:{Guid.NewGuid()}",
            DaprJobSchedule.FromDateTime(DateTimeOffset.UtcNow),
            JsonSerializer.SerializeToUtf8Bytes(email),
            cancellationToken: cancellationToken);
    }

    private async Task<byte[]> GetOrCreateInvoiceAsync(BookingInvoiceData booking, string invoiceFileName, CancellationToken cancellationToken)
    {
        var outputDirectory = "invoices";
        Directory.CreateDirectory(outputDirectory);
        var invoicePath = Path.Combine(outputDirectory, invoiceFileName);

        if (File.Exists(invoicePath))
        {
            return await File.ReadAllBytesAsync(invoicePath, cancellationToken);
        }

        var invoiceBytes = invoiceService.BuildInvoice(booking);
        await File.WriteAllBytesAsync(invoicePath, invoiceBytes, cancellationToken);
        return invoiceBytes;
    }
}

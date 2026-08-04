using Core.Application.Common.Exceptions;
using Core.Application.Resources;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using Shared.Domain.Models;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>Encola el correo que confirma al cliente la cancelación de su reserva.</summary>
/// <param name="bookingRepository">Repositorio del que se obtienen los datos de la reserva.</param>
/// <param name="jobService">Servicio que publica el correo en la cola de envíos.</param>
/// <param name="logger">Registro de los motivos por los que no se envía el correo.</param>
internal class SendBookingCanceledEmailActivity(
    IBookingRepository bookingRepository,
    IJobService jobService,
    ILogger<SendBookingCanceledEmailActivity> logger) : WorkflowActivity<int, bool>
{
    readonly IBookingRepository _bookingRepository = bookingRepository;
    readonly IJobService _jobService = jobService;
    readonly ILogger<SendBookingCanceledEmailActivity> _logger = logger;

    /// <summary>Encola el correo de cancelación de la reserva indicada.</summary>
    /// <param name="context">Contexto de ejecución de la activity.</param>
    /// <param name="input">Identificador de la reserva.</param>
    /// <returns><c>true</c> si el correo se ha encolado; <c>false</c> si el usuario no tiene email.</returns>
    /// <exception cref="NotFoundException">La reserva no existe.</exception>
    public override async Task<bool> RunAsync(WorkflowActivityContext context, int input)
    {
        var booking = await _bookingRepository.GetBookingEmailData(input)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFoundInWorkflow, input));

        if (string.IsNullOrWhiteSpace(booking.UserEmail))
        {
            _logger.LogWarning(
                "No se envía el correo de cancelación de la reserva {BookingId}: el usuario no tiene email.",
                input);

            return false;
        }

        var cruiseName = booking.CruiseName ?? "tu crucero";
        var startDate = booking.StartDate.ToString("dd/MM/yyyy");

        await _jobService.EnqueueEmailAsync(new EmailData
        {
            To = booking.UserEmail,
            Subject = $"Tu reserva #{booking.Id} ha sido cancelada",
            HtmlBody = $"""
                <p>Hola {booking.UserName},</p>
                <p>Confirmamos la cancelación de tu reserva <strong>#{booking.Id}</strong>.</p>
                <p>Crucero: {cruiseName}<br/>Salida: {startDate}</p>
                <p>Esperamos verte pronto en otro de nuestros cruceros.</p>
                <p>Gracias por confiar en Cruise Booking.</p>
                """
        });

        return true;
    }
}

using Core.Application.Common.Exceptions;
using Core.Application.Resources;
using Dapr.Workflow;
using Shared.Domain.Models;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Infrastructure.Options;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>Encola el correo que avisa del fallo del cobro automático, con un enlace a la página de pago manual.</summary>
/// <param name="bookingRepository">Repositorio del que se obtienen los datos de la reserva.</param>
/// <param name="jobService">Servicio que publica el correo en la cola de envíos.</param>
/// <param name="checkInOptions">Opciones con la URL base del front de pago.</param>
/// <param name="logger">Registro de los motivos por los que no se envía el correo.</param>
internal class SendPaymentErrorEmailActivity(
    IBookingRepository bookingRepository,
    IJobService jobService,
    IOptions<CheckInOptions> checkInOptions,
    ILogger<SendPaymentErrorEmailActivity> logger) : WorkflowActivity<int, bool>
{
    readonly IBookingRepository _bookingRepository = bookingRepository;
    readonly IJobService _jobService = jobService;
    readonly CheckInOptions _checkInOptions = checkInOptions.Value;
    readonly ILogger<SendPaymentErrorEmailActivity> _logger = logger;

    /// <summary>Encola el correo de error de cobro de la reserva indicada.</summary>
    /// <param name="context">Contexto de ejecución de la activity.</param>
    /// <param name="input">Identificador de la reserva.</param>
    /// <returns><c>true</c> si el correo se ha encolado; <c>false</c> si la reserva está cancelada o el usuario no tiene email.</returns>
    /// <exception cref="NotFoundException">La reserva no existe.</exception>
    public override async Task<bool> RunAsync(WorkflowActivityContext context, int input)
    {
        var booking = await _bookingRepository.GetBookingEmailData(input)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFoundInWorkflow, input));

        if (booking.IsCanceled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(booking.UserEmail))
        {
            _logger.LogWarning(
                "No se envía el correo de error de cobro de la reserva {BookingId}: el usuario no tiene email.",
                input);

            return false;
        }

        var paymentUrl = $"{_checkInOptions.FrontendBaseUrl.TrimEnd('/')}/bookings/{input}/pay";
        var cruiseName = booking.CruiseName ?? "tu crucero";
        var paymentDeadline = booking.CheckInStartDate.ToString("dd/MM/yyyy");

        await _jobService.EnqueueEmailAsync(new EmailData
        {
            To = booking.UserEmail,
            Subject = $"No hemos podido procesar el pago de tu reserva #{booking.Id}",
            HtmlBody = $"""
                <p>Hola {booking.UserName},</p>
                <p>El cobro automático de tu reserva <strong>#{booking.Id}</strong> ({cruiseName}) ha fallado.</p>
                <p>Realiza el pago manualmente antes del <strong>{paymentDeadline}</strong>. Pasada esa fecha, la reserva se cancelará automáticamente.</p>
                <p><a href="{paymentUrl}">Paga tu reserva aquí</a></p>
                <p>Gracias por confiar en Cruise Booking.</p>
                """
        });

        return true;
    }
}

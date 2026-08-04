using Core.Application.Common;
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

/// <summary>Genera el token de check-in y encola el correo que permite al cliente realizarlo.</summary>
/// <param name="bookingRepository">Repositorio del que se obtienen los datos de la reserva.</param>
/// <param name="jobService">Servicio que publica el correo en la cola de envíos.</param>
/// <param name="checkInOptions">Opciones con la URL base del front de check-in.</param>
/// <param name="logger">Registro de los motivos por los que no se envía el correo.</param>
internal class SendCheckInEmailActivity(
    IBookingRepository bookingRepository,
    IJobService jobService,
    IOptions<CheckInOptions> checkInOptions,
    ILogger<SendCheckInEmailActivity> logger) : WorkflowActivity<int, bool>
{
    readonly IBookingRepository _bookingRepository = bookingRepository;
    readonly IJobService _jobService = jobService;
    readonly CheckInOptions _checkInOptions = checkInOptions.Value;
    readonly ILogger<SendCheckInEmailActivity> _logger = logger;

    /// <summary>Encola el correo de check-in de la reserva indicada.</summary>
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
                "No se envía el correo de check-in de la reserva {BookingId}: el usuario no tiene email.",
                input);

            return false;
        }

        var token = CheckInTokens.GenerateToken();
        await _bookingRepository.SetCheckInTokenHash(input, CheckInTokens.ComputeHash(token));

        var checkInUrl = $"{_checkInOptions.FrontendBaseUrl.TrimEnd('/')}/check-in?token={token}";
        var cruiseName = booking.CruiseName ?? "tu crucero";
        var startDate = booking.StartDate.ToString("dd/MM/yyyy");

        await _jobService.EnqueueEmailAsync(new EmailData
        {
            To = booking.UserEmail,
            Subject = $"Ya puedes hacer el check-in de tu reserva #{booking.Id}",
            HtmlBody = $"""
                <p>Hola {booking.UserName},</p>
                <p>El check-in online de tu reserva <strong>#{booking.Id}</strong> ya está disponible.</p>
                <p>Crucero: {cruiseName}<br/>Salida: {startDate}</p>
                <p><a href="{checkInUrl}">Haz el check-in aquí</a></p>
                <p>Gracias por confiar en Cruise Booking.</p>
                """
        });

        return true;
    }
}

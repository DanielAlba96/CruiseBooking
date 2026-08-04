using Core.Application.Common.Exceptions;
using Core.Application.Resources;
using Dapr.Workflow;
using Shared.Domain.Repositories;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>
/// Actividad de flujo de trabajo que recupera los datos de una reserva a partir de su identificador.
/// </summary>
internal class GetBookingActivity(IBookingRepository bookingRepository) : WorkflowActivity<int, BookingWorkflowData>
{
    readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <summary>
    /// Ejecuta la actividad para obtener los datos de la reserva (fechas de cobro y check-in).
    /// </summary>
    /// <param name="context">Contexto de la actividad del flujo de trabajo.</param>
    /// <param name="input">Identificador de la reserva a recuperar.</param>
    /// <returns>Datos de la reserva con su identificador, fecha de cobro y fecha de inicio de check-in.</returns>
    /// <exception cref="NotFoundException">Se lanza cuando la reserva no existe en el sistema.</exception>
    public override async Task<BookingWorkflowData> RunAsync(WorkflowActivityContext context, int input)
    {
        var dates = await _bookingRepository.GetBookingWorkflowDates(input)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFoundInWorkflow, input));

        return new BookingWorkflowData(input, dates.ChargeDate, dates.CheckInStartDate);
    }
}

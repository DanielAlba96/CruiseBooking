using Dapr.Workflow;
using Shared.Domain.Repositories;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>
/// Actividad de flujo de trabajo que cancela reservas no pagadas después de un período de tiempo.
/// </summary>
internal class CancelUnpaidBookingActivity(IBookingRepository bookingRepository) : WorkflowActivity<int, bool>
{
    readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <summary>
    /// Ejecuta la actividad para cancelar una reserva no pagada.
    /// </summary>
    /// <param name="context">Contexto de ejecución de la actividad del flujo de trabajo.</param>
    /// <param name="input">Identificador de la reserva a cancelar.</param>
    /// <returns>
    /// Una tarea que devuelve <c>true</c> si la reserva fue cancelada exitosamente;
    /// en caso contrario, <c>false</c>.
    /// </returns>
    public override Task<bool> RunAsync(WorkflowActivityContext context, int input)
    {
        return _bookingRepository.CancelUnpaidBooking(input);
    }
}

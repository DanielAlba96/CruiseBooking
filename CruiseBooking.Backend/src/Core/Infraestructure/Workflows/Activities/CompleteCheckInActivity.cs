using Dapr.Workflow;
using Shared.Domain.Repositories;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>
/// Actividad de flujo de trabajo que marca el check-in de una reserva como completado.
/// </summary>
internal class CompleteCheckInActivity(IBookingRepository bookingRepository) : WorkflowActivity<int, bool>
{
    readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <summary>
    /// Ejecuta la actividad para marcar el check-in como completado en la reserva indicada.
    /// </summary>
    /// <param name="context">Contexto de la actividad del flujo de trabajo.</param>
    /// <param name="input">Identificador de la reserva.</param>
    /// <returns><see langword="true"/> si el check-in se marcó como completado; <see langword="false"/> si no se pudo actualizar.</returns>
    public override Task<bool> RunAsync(WorkflowActivityContext context, int input)
    {
        return _bookingRepository.CompleteCheckIn(input);
    }
}

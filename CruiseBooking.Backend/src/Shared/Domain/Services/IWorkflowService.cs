namespace Shared.Domain.Services;

/// <summary>
/// Define el servicio para orquestar flujos de trabajo de reservas.
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Inicia el flujo de trabajo de reserva asociado a un identificador de reserva.
    /// </summary>
    /// <param name="bookingId">Identificador único de la reserva.</param>
    /// <returns>Tarea que representa la operación asincrónica.</returns>
    Task StartBookingWorkflow(int bookingId);

    /// <summary>
    /// Envía un evento de cancelación de reserva al flujo de trabajo.
    /// </summary>
    /// <param name="bookingId">Identificador único de la reserva a cancelar.</param>
    /// <returns>Tarea que representa la operación asincrónica.</returns>
    Task SendBookingCanceledEvent(int bookingId);

    /// <summary>
    /// Envía un evento indicando que el proceso de check-in ha sido completado.
    /// </summary>
    /// <param name="bookingId">Identificador único de la reserva.</param>
    /// <returns>Tarea que representa la operación asincrónica.</returns>
    Task SendCheckInCompletedEvent(int bookingId);

    /// <summary>
    /// Envía un evento de pago manual al flujo de trabajo.
    /// </summary>
    /// <param name="bookingId">Identificador único de la reserva.</param>
    /// <returns>Tarea que representa la operación asincrónica.</returns>
    Task SendManualPaymentEvent(int bookingId);
}

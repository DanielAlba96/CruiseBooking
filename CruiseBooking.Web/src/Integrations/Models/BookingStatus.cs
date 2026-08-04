namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa el estado de una reserva en su ciclo de vida.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// La reserva ha sido creada pero aún no ha sido pagada.
    /// </summary>
    Creada = 1,

    /// <summary>
    /// La reserva está esperando a que se reciba el pago.
    /// </summary>
    PagoPendiente = 2,

    /// <summary>
    /// La reserva ha sido pagada pero el check-in aún no se ha completado.
    /// </summary>
    CheckinPendiente = 3,

    /// <summary>
    /// La reserva ha sido completada con check-in realizado.
    /// </summary>
    Completada = 4,

    /// <summary>
    /// La reserva ha sido cancelada.
    /// </summary>
    Cancelada = 5
}

namespace Shared.Domain.Models;

/// <summary>
/// Proyección mínima de una reserva con los campos necesarios para componer los emails del workflow.
/// </summary>
/// <param name="Id">Identificador de la reserva.</param>
/// <param name="IsCanceled">Indica si la reserva está cancelada.</param>
/// <param name="UserName">Nombre del usuario de la reserva.</param>
/// <param name="UserEmail">Email del usuario de la reserva.</param>
/// <param name="CruiseName">Nombre del crucero.</param>
/// <param name="StartDate">Fecha de salida del crucero.</param>
/// <param name="CheckInStartDate">Fecha límite de pago manual / inicio del check-in.</param>
public sealed record BookingEmailData(
    int Id,
    bool IsCanceled,
    string UserName,
    string UserEmail,
    string? CruiseName,
    DateTime StartDate,
    DateTime CheckInStartDate);

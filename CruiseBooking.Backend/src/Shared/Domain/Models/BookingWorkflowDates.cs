namespace Shared.Domain.Models;

/// <summary>
/// Fechas del crucero relevantes para el workflow de reserva: cobro automático y check-in.
/// </summary>
/// <param name="ChargeDate">Fecha límite del intento de cobro automático.</param>
/// <param name="CheckInStartDate">Fecha límite de pago manual / inicio del check-in.</param>
public sealed record BookingWorkflowDates(DateTime ChargeDate, DateTime CheckInStartDate);

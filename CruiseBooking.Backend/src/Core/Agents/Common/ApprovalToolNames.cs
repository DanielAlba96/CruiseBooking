namespace Core.Agents.Common;

/// <summary>
/// Nombres de las herramientas que requieren aprobación explícita del usuario antes de ejecutarse.
/// Estos valores se usan como <c>ToolName</c> de la solicitud de aprobación, como discriminador
/// de tipo en la serialización polimórfica y como clave de los servicios registrados con
/// <c>AddKeyedScoped</c>.
/// </summary>
internal static class ApprovalToolNames
{
    /// <summary>
    /// Confirmación de la reserva en curso.
    /// </summary>
    public const string ConfirmBooking = "confirm_booking";

    /// <summary>
    /// Cobro manual de una reserva pendiente de pago.
    /// </summary>
    public const string PayBooking = "pay_booking";

    /// <summary>
    /// Cancelación de una reserva existente.
    /// </summary>
    public const string CancelBooking = "cancel_booking";
}

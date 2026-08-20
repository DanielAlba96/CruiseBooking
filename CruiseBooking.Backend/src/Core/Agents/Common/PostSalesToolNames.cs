namespace Core.Agents.Common;

/// <summary>
/// Nombres con los que las herramientas de post-venta se exponen al modelo de lenguaje.
/// Coinciden con el <see cref="System.ComponentModel.DisplayNameAttribute"/> de cada método de
/// <c>PostSalesTools</c> y son los que aparecen en <c>FunctionCallContent.Name</c>.
/// <see cref="PayBooking"/> y <see cref="CancelBooking"/> se usan además como <c>ToolName</c> de la
/// solicitud de aprobación, como discriminador de tipo en la serialización polimórfica y como clave
/// de los servicios registrados con <c>AddKeyedScoped</c>.
/// </summary>
internal static class PostSalesToolNames
{
    /// <summary>
    /// Listado de las reservas del usuario.
    /// </summary>
    public const string GetMyBookings = "get_my_bookings";

    /// <summary>
    /// Detalle de una reserva existente.
    /// </summary>
    public const string GetBookingDetail = "get_booking_detail";

    /// <summary>
    /// Listado de los métodos de pago del usuario.
    /// </summary>
    public const string GetPaymentMethods = "get_payment_methods";

    /// <summary>
    /// Cobro manual de una reserva pendiente de pago. Requiere aprobación explícita del usuario.
    /// </summary>
    public const string PayBooking = "pay_booking";

    /// <summary>
    /// Cancelación de una reserva existente. Requiere aprobación explícita del usuario.
    /// </summary>
    public const string CancelBooking = "cancel_booking";
}

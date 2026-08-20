using Core.Agents.Common;
using System.Text.Json.Serialization;

namespace Core.Agents.Models;

/// <summary>
/// Solicitud de aprobación pendiente asociada a una sesión de chat.
/// La jerarquía se serializa de forma polimórfica para que la lectura desde caché
/// devuelva el tipo concreto con todos sus datos. Para que el discriminador se escriba,
/// el valor debe declararse siempre como <see cref="ToolApprovalRequest"/> al guardarlo.
/// </summary>
/// <param name="CallId">Identificador único de la solicitud, devuelto al cliente para confirmarla o rechazarla.</param>
/// <param name="ToolName">Nombre de la herramienta que solicitó la aprobación.</param>
/// <param name="ApprovalMessage">Resumen en Markdown que se muestra al usuario para que decida.</param>
[JsonPolymorphic]
[JsonDerivedType(typeof(ConfirmBookingApprovalRequest), BookingToolNames.ConfirmBooking)]
[JsonDerivedType(typeof(PayApprovalRequest), PostSalesToolNames.PayBooking)]
[JsonDerivedType(typeof(CancelBookingApprovalRequest), PostSalesToolNames.CancelBooking)]
internal record ToolApprovalRequest(string CallId, string ToolName, string ApprovalMessage);

/// <summary>
/// Solicitud de aprobación para confirmar la reserva en curso.
/// </summary>
/// <param name="CallId">Identificador único de la solicitud.</param>
/// <param name="ToolName">Nombre de la herramienta que solicitó la aprobación.</param>
/// <param name="ApprovalMessage">Resumen en Markdown de la reserva a confirmar.</param>
internal sealed record ConfirmBookingApprovalRequest(string CallId, string ToolName, string ApprovalMessage) : ToolApprovalRequest(CallId, ToolName, ApprovalMessage);

/// <summary>
/// Solicitud de aprobación para cobrar una reserva pendiente de pago.
/// </summary>
/// <param name="CallId">Identificador único de la solicitud.</param>
/// <param name="ToolName">Nombre de la herramienta que solicitó la aprobación.</param>
/// <param name="ApprovalMessage">Resumen en Markdown del cobro a realizar.</param>
/// <param name="BookingId">Identificador de la reserva a cobrar.</param>
/// <param name="PaymentMethodId">Identificador del método de pago elegido por el usuario.</param>
internal sealed record PayApprovalRequest(string CallId, string ToolName, string ApprovalMessage, int BookingId, string PaymentMethodId) : ToolApprovalRequest(CallId, ToolName, ApprovalMessage);

/// <summary>
/// Solicitud de aprobación para cancelar una reserva existente.
/// </summary>
/// <param name="CallId">Identificador único de la solicitud.</param>
/// <param name="ToolName">Nombre de la herramienta que solicitó la aprobación.</param>
/// <param name="ApprovalMessage">Resumen en Markdown de la cancelación a realizar.</param>
/// <param name="BookingId">Identificador de la reserva a cancelar.</param>
internal sealed record CancelBookingApprovalRequest(string CallId, string ToolName, string ApprovalMessage, int BookingId) : ToolApprovalRequest(CallId, ToolName, ApprovalMessage);

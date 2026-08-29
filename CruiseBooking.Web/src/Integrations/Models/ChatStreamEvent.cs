namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa un evento recibido a través del stream Server-Sent Events del chat.
/// El nombre del evento SSE actúa como discriminador del tipo concreto.
/// </summary>
public abstract record ChatStreamEvent;

/// <summary>
/// Fragmento de texto generado por el asistente (evento <c>token</c>).
/// </summary>
/// <param name="Text">Texto parcial que debe concatenarse a la respuesta en curso.</param>
public sealed record ChatStreamEventToken(string Text) : ChatStreamEvent;

/// <summary>
/// Solicitud de aprobación de una herramienta antes de ejecutarse (evento <c>approval_required</c>).
/// </summary>
/// <param name="CallId">Identificador de la llamada a la herramienta, necesario para enviar la decisión.</param>
/// <param name="Summary">Resumen estructurado de la operación pendiente de aprobación.</param>
public sealed record ChatStreamEventApproval(string CallId, ApprovalSummary Summary) : ChatStreamEvent;

/// <summary>
/// Error emitido por el asistente durante el stream (evento <c>failed</c>).
/// </summary>
/// <param name="Reason">Motivo del error, apto para mostrarse al usuario.</param>
public sealed record ChatStreamEventError(string Reason) : ChatStreamEvent;

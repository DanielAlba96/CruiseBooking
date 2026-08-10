namespace Core.Agents.Common;

/// <summary>
/// Centraliza las claves de caché utilizadas por la conversación del asistente.
/// Todas las claves cuelgan del prefijo <c>chat:{sessionId}</c>.
/// </summary>
internal static class ChatCacheKeyReference
{
    private const string Prefix = "chat";
    private const string MessagesSuffix = "messages";
    private const string StateSuffix = "state";
    private const string DraftSuffix = "draft";
    private const string ApprovalSuffix = "approval";
    private const string ManualApprovalSuffix = "approval-pending";

    /// <summary>
    /// Clave del historial de mensajes de la conversación.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión de chat.</param>
    /// <returns>Clave de caché del historial de mensajes.</returns>
    public static string Messages(Guid sessionId) => $"{Prefix}:{sessionId}:{MessagesSuffix}";

    /// <summary>
    /// Clave del estado serializado de la sesión del agente o del workflow.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión de chat.</param>
    /// <returns>Clave de caché del estado de la sesión.</returns>
    public static string State(Guid sessionId) => $"{Prefix}:{sessionId}:{StateSuffix}";

    /// <summary>
    /// Clave del borrador de reserva en curso.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión de chat.</param>
    /// <returns>Clave de caché del borrador de reserva.</returns>
    public static string Draft(Guid sessionId) => $"{Prefix}:{sessionId}:{DraftSuffix}";

    /// <summary>
    /// Clave de la solicitud de aprobación de MAF asociada a una llamada de herramienta concreta.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión de chat.</param>
    /// <param name="toolCallId">Identificador de la llamada a la herramienta.</param>
    /// <returns>Clave de caché de la solicitud de aprobación.</returns>
    public static string MAFApproval(Guid sessionId, string toolCallId) => $"{Prefix}:{sessionId}:{ApprovalSuffix}:{toolCallId}";

    /// <summary>
    /// Clave de la solicitud de aprobación pendiente gestionada manualmente fuera de MAF.
    /// </summary>
    /// <param name="sessionId">Identificador de la sesión de chat.</param>
    /// <returns>Clave de caché de la aprobación pendiente.</returns>
    public static string ManualApproval(Guid sessionId) => $"{Prefix}:{sessionId}:{ManualApprovalSuffix}";
}

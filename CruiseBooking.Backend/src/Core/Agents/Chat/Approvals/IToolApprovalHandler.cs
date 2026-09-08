using Core.Agents.Models;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Resuelve la decisión del usuario sobre una herramienta que quedó pendiente de aprobación.
/// Cada herramienta aprobable tiene su propia implementación, registrada con
/// <c>AddKeyedScoped</c> usando el nombre de la herramienta como clave.
/// </summary>
internal interface IToolApprovalHandler
{
    /// <summary>
    /// Aplica la decisión del usuario sobre la solicitud de aprobación.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="request">Solicitud de aprobación recuperada de la caché.</param>
    /// <param name="approved">Resultado de la aprobación.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación asincrónica.</param>
    /// <exception cref="InvalidOperationException">
    /// La solicitud recuperada de la caché no corresponde al tipo que atiende el manejador.
    /// </exception>
    Task HandleAsync(Guid sessionId, ToolApprovalRequest request, bool approved, CancellationToken ct = default);
}

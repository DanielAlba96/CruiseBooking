using CruiseBooking.Integrations.Models;
using Refit;

namespace CruiseBooking.Integrations;

public interface IChatApi
{
    /// <summary>
    /// Envía un mensaje a la sesión de chat y devuelve el cuerpo de la respuesta
    /// como stream Server-Sent Events. Refit usa <c>ResponseHeadersRead</c> para los
    /// retornos <see cref="Stream"/>, por lo que el stream se consume token a token.
    /// </summary>
    [Post("/chat/sessions/{sessionId}")]
    Task<Stream> StreamChat(Guid sessionId, [Body] ChatMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía la decisión del usuario sobre una operación pendiente de aprobación y devuelve
    /// la continuación de la conversación como stream Server-Sent Events.
    /// </summary>
    [Post("/chat/sessions/{sessionId}/approvals/{callId}")]
    Task<Stream> SubmitApproval(Guid sessionId, string callId, [Body] ApprovalDecisionRequest request, CancellationToken cancellationToken = default);
}

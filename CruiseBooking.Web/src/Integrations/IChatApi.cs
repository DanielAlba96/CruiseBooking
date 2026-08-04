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
}

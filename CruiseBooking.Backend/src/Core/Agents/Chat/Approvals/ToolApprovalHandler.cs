using Core.Agents.Models;
using Core.Agents.Resources;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Base de los manejadores de aprobación que concentra la conversión al tipo concreto
/// de la solicitud, de forma que las implementaciones trabajen ya tipadas.
/// Una entrada de caché anterior a la serialización polimórfica se deserializa como
/// <see cref="ToolApprovalRequest"/> base y aterriza aquí, provocando una excepción que
/// resuelve el llamador.
/// </summary>
/// <typeparam name="TRequest">Tipo concreto de la solicitud que atiende el manejador.</typeparam>
internal abstract class ToolApprovalHandler<TRequest> : IToolApprovalHandler
    where TRequest : ToolApprovalRequest
{
    /// <inheritdoc />
    public Task HandleAsync(Guid sessionId, ToolApprovalRequest request, bool approved, CancellationToken ct = default)
    {
        return request is not TRequest typedRequest
            ? throw new InvalidOperationException(string.Format(ErrorMessages.ApprovalRequestTypeMismatch, request.ToolName, typeof(TRequest).Name))
            : HandleCoreAsync(sessionId, typedRequest, approved, ct);
    }

    /// <summary>
    /// Aplica la decisión del usuario sobre una solicitud ya tipada.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="request">Solicitud de aprobación del tipo que atiende el manejador.</param>
    /// <param name="approved">Resultado de la aprobación.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación asincrónica.</param>
    protected abstract Task HandleCoreAsync(Guid sessionId, TRequest request, bool approved, CancellationToken ct);
}

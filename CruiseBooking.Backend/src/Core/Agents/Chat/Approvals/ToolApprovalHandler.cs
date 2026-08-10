using Core.Agents.Models;

namespace Core.Agents.Chat.Approvals;

/// <summary>
/// Base de los manejadores de aprobación que concentra la conversión al tipo concreto
/// de la solicitud, de forma que las implementaciones trabajen ya tipadas.
/// Una entrada de caché anterior a la serialización polimórfica se deserializa como
/// <see cref="ToolApprovalRequest"/> base y aterriza aquí, devolviendo el mensaje de error controlado.
/// </summary>
/// <typeparam name="TRequest">Tipo concreto de la solicitud que atiende el manejador.</typeparam>
internal abstract class ToolApprovalHandler<TRequest> : IToolApprovalHandler
    where TRequest : ToolApprovalRequest
{
    /// <inheritdoc />
    public Task<string> HandleAsync(Guid sessionId, ToolApprovalRequest request, bool approved, CancellationToken ct = default)
        => request is TRequest typedRequest
            ? HandleCoreAsync(sessionId, typedRequest, approved, ct)
            : Task.FromResult(IToolApprovalHandler.ApprovalFailedMessage);

    /// <summary>
    /// Aplica la decisión del usuario sobre una solicitud ya tipada.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="request">Solicitud de aprobación del tipo que atiende el manejador.</param>
    /// <param name="approved">Resultado de la aprobación.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación asincrónica.</param>
    /// <returns>El mensaje de usuario que se reinyecta en la conversación para que el agente continúe.</returns>
    protected abstract Task<string> HandleCoreAsync(Guid sessionId, TRequest request, bool approved, CancellationToken ct);
}

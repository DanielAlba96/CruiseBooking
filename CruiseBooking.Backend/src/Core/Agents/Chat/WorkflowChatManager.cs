using Core.Agents.Chat.Approvals;
using Core.Agents.Common;
using Core.Agents.Models;
using Core.Agents.Orchestration;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Services;
using System.Runtime.CompilerServices;

namespace Core.Agents.Chat;

/// <summary>
/// Gestor de chat en forma de workflow con handoff.
/// Un agente de triage delega en 2 especialistas segun la petición del usuario.
/// En el momento del desarrollo existe un bug en MAF que impide solicitar
/// aprobacion para ejecutar herramientas cuando se usan workflows con handoff,
/// asi que hay que hacerlo manualmente fuera de MAF.
/// <see href="https://github.com/microsoft/agent-framework/issues/5621">Ver issue en GitHub</see>.
/// </summary>
internal class WorkflowChatManager(
    IWorkflowFactory workflowFactory,
    ICacheService cacheService,
    UserInfo userInfo,
    IServiceProvider serviceProvider,
    IChatReducer reducer) : IChatManager
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserInfo _userInfo = userInfo;
    private readonly IWorkflowFactory _workflowFactory = workflowFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IChatReducer _reducer = reducer;

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamEvent> StartChatStreamAsync(
        Guid sessionId,
        string message,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var workflowSession = await LoadSessionAsync(sessionId);

        await foreach (var e in StreamChatCoreAsync(workflowSession, new ChatMessage(ChatRole.User, message), ct))
            yield return e;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamEvent> StartChatStreamWithApprovalAsync(
        Guid sessionId,
        string toolCallId,
        bool approved, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var workflowSession = await LoadSessionAsync(sessionId);

        var approvalKey = ChatCacheKeyReference.ManualApproval(sessionId);

        var approvalRequest = await _cacheService.GetAsync<ToolApprovalRequest>(approvalKey, ct);
        if (approvalRequest is null || !approvalRequest.CallId.Equals(toolCallId))
            throw new NotFoundException("No se encuentra la aprobación");

        var handler = _serviceProvider.GetKeyedService<IToolApprovalHandler>(approvalRequest.ToolName);

        string message = handler is null
            ? IToolApprovalHandler.ApprovalFailedMessage
            : await handler.HandleAsync(sessionId, approvalRequest, approved, ct);

        await _cacheService.RemoveAsync(approvalKey, ct);

        await foreach (var e in StreamChatCoreAsync(workflowSession, new ChatMessage(ChatRole.User, message), ct))
            yield return e;
    }

    private async IAsyncEnumerable<ChatStreamEvent> StreamChatCoreAsync(
        WorkflowSession workflowSession,
        ChatMessage message,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        workflowSession.Messages.Add(message);

        var workflow = _workflowFactory.CreateChatWorkflow(workflowSession.SessionId);
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, workflowSession.Messages, cancellationToken: ct);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        List<ChatMessage>? newMessages = [];
        await foreach (var evt in run.WatchStreamAsync(ct))
        {
            switch (evt)
            {
                case AgentResponseUpdateEvent agentResponse:
                    if (!string.IsNullOrEmpty(agentResponse.Update.Text))
                        yield return new ChatStreamEventToken(agentResponse.Update.Text);
                    break;

                case WorkflowOutputEvent output when output.Is<List<ChatMessage>>():
                    newMessages = output.As<List<ChatMessage>>();
                    break;

                case WorkflowErrorEvent workflowError:
                    yield return new ChatStreamEventError(
                        workflowError.Exception?.Message ?? "Se ha producido un error en la conversación.");
                    break;

                case ExecutorFailedEvent executorFailed:
                    yield return new ChatStreamEventError(
                        $"El agente '{executorFailed.ExecutorId}' no ha podido completar la operación.");
                    break;
            }
        }

        if (newMessages is not null)
            workflowSession.Messages.AddRange(newMessages.Skip(workflowSession.Messages.Count));

        var approvalRequest = await _cacheService.GetAsync<ToolApprovalRequest>(ChatCacheKeyReference.ManualApproval(workflowSession.SessionId), ct);
        if (approvalRequest is not null)
        {
            workflowSession.Messages.Add(new ChatMessage(ChatRole.Assistant, $"La herramienta {approvalRequest.ToolName} ha solicitado aprobación al usuario. Quedas a la espera de que el usuario te indique el resultado."));

            yield return new ChatStreamEventApproval(approvalRequest.CallId, approvalRequest.ApprovalMessage);
        }

        var compactedMessages = await _reducer.ReduceAsync(workflowSession.Messages, ct);
        workflowSession = workflowSession with
        {
            Messages = [.. compactedMessages]
        };

        await _cacheService.SetAsync(ChatCacheKeyReference.State(workflowSession.SessionId), workflowSession,
            TimeSpan.FromMinutes(10),
            CancellationToken.None);
    }

    /// <summary>
    /// Recupera la sesión almacenada y valida que pertenezca al usuario actual y sea coherente.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <returns>La sesión almacenada, o <c>null</c> si todavía no existe.</returns>
    /// <exception cref="UnauthorizedAccessException">La sesión pertenece a otro usuario.</exception>
    /// <exception cref="InvalidOperationException">La sesión almacenada no es coherente.</exception>
    private async Task<WorkflowSession> LoadSessionAsync(Guid sessionId)
    {
        var workflowSession = await _cacheService.GetAsync<WorkflowSession>(ChatCacheKeyReference.State(sessionId), CancellationToken.None);
        if (workflowSession is null)
            return new WorkflowSession(sessionId, _userInfo.Id, []);

        if (workflowSession.UserId != _userInfo.Id)
            throw new UnauthorizedAccessException("No tiene permiso para acceder a esta conversación.");

        if (workflowSession.SessionId != sessionId)
            throw new InvalidOperationException("El Id de sesión no coincide con la sesión almacenada.");

        if (workflowSession.Messages is null)
            throw new InvalidOperationException("La sesión de conversación no contiene mensajes.");

        return workflowSession;
    }
}

using Core.Agents.Models;
using Core.Agents.Orchestration;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Shared.Domain.Services;
using System.Runtime.CompilerServices;

namespace Core.Agents.Chat;

/// <summary>
/// Gestor de chat en forma de workflow con handoff.
/// Un agente de triage delega en 2 especialistas segun la petición del usuario
/// En el momento del desarrollo existe un bug en MAF que impide solicitar
/// aprobacion para ejecutar herramientas cuando se usan workflows con handoff,
/// asi que hay que hacer un workaround manual.
/// <see href="https://github.com/microsoft/agent-framework/issues/5621">Ver issue en GitHub</see>.
/// </summary>
internal class WorkflowChatManager(
    IWorkflowFactory workflowFactory,
    ICacheService cacheService,
    UserInfo userInfo) : IChatManager
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserInfo _userInfo = userInfo;
    private readonly IWorkflowFactory _workflowFactory = workflowFactory;

    /// <inheritdoc />
    public IAsyncEnumerable<ChatStreamEvent> StartChatStreamAsync(Guid sessionId, string message, CancellationToken ct = default)
        => StreamChatCoreAsync(sessionId, new ChatMessage(ChatRole.User, message), ct);

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamEvent> StartChatStreamWithApprovalAsync(
        Guid sessionId,
        string toolCallId,
        bool approved,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var approvalRequest = await _cacheService.GetAsync<ToolApprovalRequestContent>($"chat:{sessionId}:approval:{toolCallId}", ct)
            ?? throw new NotFoundException("Esta reserva ya no está disponible para su aprobación");

        var message = new ChatMessage(ChatRole.User, [approvalRequest.CreateResponse(approved)]);
        await foreach (var e in StreamChatCoreAsync(sessionId, message, ct))
            yield return e;
    }

    private async IAsyncEnumerable<ChatStreamEvent> StreamChatCoreAsync(Guid sessionId, ChatMessage message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var workflowSession = await _cacheService.GetAsync<WorkflowSession>($"chat:{sessionId}:state", CancellationToken.None);

        List<ChatMessage> sessionMessages = [];
        if (workflowSession is not null)
        {
            if (workflowSession.UserId != _userInfo.Id)
                throw new UnauthorizedAccessException("No tiene permiso para acceder a esta conversación.");

            if (workflowSession.SessionId != sessionId)
                throw new InvalidOperationException("El Id de sesión no coincide con la sesión almacenada.");

            if (workflowSession.Messages is null)
                throw new InvalidOperationException("La sesión de conversación no contiene mensajes.");

            sessionMessages = workflowSession.Messages;
        }

        sessionMessages.Add(message);

        var workflow = _workflowFactory.CreateChatWorkflow(sessionId);
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, sessionMessages, cancellationToken: ct);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        List<ChatMessage>? newMessages = [];
        await foreach (var evt in run.WatchStreamAsync(ct))
        {
            switch(evt)
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
            sessionMessages.AddRange(newMessages.Skip(sessionMessages.Count));

        workflowSession = new WorkflowSession(sessionId, _userInfo.Id, sessionMessages);
        await _cacheService.SetAsync($"chat:{sessionId}:state", workflowSession,
            TimeSpan.FromMinutes(10),
            CancellationToken.None);
    }
}

using Core.Agents.Chat.Approvals;
using Core.Agents.Chat.Orchestration;
using Core.Agents.Common;
using Core.Agents.Models;
using Core.Agents.Resources;
using Core.Application.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Domain.Services;
using System.Runtime.CompilerServices;

namespace Core.Agents.Chat;

/// <summary>
/// Gestor de chat en forma de workflow con handoff.
/// Un agente de triage delega en 2 especialistas segun la petición del usuario.
/// En el momento del desarrollo existe un bug en MAF que impide solicitar
/// aprobacion para ejecutar herramientas cuando se usan workflows con handoff
/// por un problema en los checkpoints, asi que usar una alternativa manual.
/// <see href="https://github.com/microsoft/agent-framework/issues/5621">Ver issue en GitHub</see>.
/// </summary>
internal class WorkflowChatManager(
    IWorkflowFactory workflowFactory,
    ICacheService cacheService,
    UserInfo userInfo,
    IServiceProvider serviceProvider,
    IChatReducer reducer,
    ILogger<WorkflowChatManager> logger) : IChatManager
{
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserInfo _userInfo = userInfo;
    private readonly IWorkflowFactory _workflowFactory = workflowFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IChatReducer _reducer = reducer;
    private readonly ILogger<WorkflowChatManager> _logger = logger;

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
        var preparation = await PrepareApprovalAsync(sessionId, toolCallId, approved, ct);
        if (preparation.Session is null || preparation.Message is null)
        {
            yield return new ChatStreamEventError(ErrorMessages.ChatStreamFailed);
            yield break;
        }

        await foreach (var e in StreamChatCoreAsync(preparation.Session, new ChatMessage(ChatRole.User, preparation.Message), ct))
            yield return e;
    }

    private async Task<ApprovalPreparation> PrepareApprovalAsync(
        Guid sessionId,
        string toolCallId,
        bool approved,
        CancellationToken ct)
    {
        var workflowSession = await LoadSessionAsync(sessionId);
        var approvalKey = ChatCacheKeyReference.ManualApproval(sessionId);

        try
        {
            var approvalRequest = await _cacheService.GetAsync<ToolApprovalRequest>(approvalKey, ct);
            if (approvalRequest is null || !approvalRequest.CallId.Equals(toolCallId))
            {
                _logger.LogWarning("No se encuentra la aprobación {ToolCallId} de la sesión {SessionId}", toolCallId, sessionId);
                return new ApprovalPreparation(null, null);
            }

            var handler = _serviceProvider.GetKeyedService<IToolApprovalHandler>(approvalRequest.ToolName);
            if (!approved)
                return new ApprovalPreparation(workflowSession, $"Rechazado: {approvalRequest.CallId}");

            await handler!.HandleAsync(sessionId, approvalRequest, approved, ct);
            return new ApprovalPreparation(workflowSession, $"Aprobado: {approvalRequest.CallId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando la aprobación {ToolCallId} de la sesión {SessionId}", toolCallId, sessionId);
            return new ApprovalPreparation(null, null);
        }
        finally
        {
            await _cacheService.RemoveAsync(approvalKey, ct);
        }
    }

    private async IAsyncEnumerable<ChatStreamEvent> StreamChatCoreAsync(
        WorkflowSession workflowSession,
        ChatMessage message,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        workflowSession.Messages.Add(message);

        LogContextSize("entrada", workflowSession.SessionId, workflowSession.Messages);

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

                case WorkflowErrorEvent:
                case ExecutorFailedEvent:
                    yield return new ChatStreamEventError(ErrorMessages.ChatStreamFailed);
                    yield break;
            }
        }

        if (newMessages is { Count: > 0 })
            workflowSession = workflowSession with { Messages = Sanitize(newMessages) };

        var approvalRequest = await _cacheService.GetAsync<ToolApprovalRequest>(ChatCacheKeyReference.ManualApproval(workflowSession.SessionId), ct);
        if (approvalRequest is not null)
        {
            workflowSession.Messages.Add(new ChatMessage(ChatRole.Assistant, $"Pendiente de aprobación del usuario: {approvalRequest.CallId}"));
            yield return new ChatStreamEventApproval(approvalRequest.CallId, approvalRequest.Summary);
        }

        var compactedMessages = await _reducer.ReduceAsync(workflowSession.Messages, ct);
        workflowSession = workflowSession with
        {
            Messages = [.. Sanitize(compactedMessages)]
        };

        await _cacheService.SetAsync(ChatCacheKeyReference.State(workflowSession.SessionId), workflowSession,
            TimeSpan.FromMinutes(10),
            CancellationToken.None);

        LogContextSize("salida", workflowSession.SessionId, workflowSession.Messages);
    }

    private async Task<WorkflowSession> LoadSessionAsync(Guid sessionId)
    {
        var workflowSession = await _cacheService.GetAsync<WorkflowSession>(ChatCacheKeyReference.State(sessionId), CancellationToken.None);
        if (workflowSession is null)
            return new WorkflowSession(sessionId, _userInfo.Id, []);

        if (workflowSession.UserId != _userInfo.Id)
            throw new UnauthorizedAccessException(ErrorMessages.SessionAccessDenied);

        if (workflowSession.SessionId != sessionId)
            throw new InvalidOperationException(ErrorMessages.SessionIdMismatch);

        if (workflowSession.Messages is null)
            throw new InvalidOperationException(ErrorMessages.SessionHasNoMessages);

        return workflowSession;
    }

    private static List<ChatMessage> Sanitize(IEnumerable<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        List<ChatMessage> sanitized = [];

        foreach (var message in messages)
        {
            var contents = message.Contents.Where(IsRelevant).ToList();
            if (contents.Count == 0)
                continue;

            if (contents.Count == message.Contents.Count)
            {
                sanitized.Add(message);
                continue;
            }

            sanitized.Add(new ChatMessage(message.Role, contents)
            {
                AuthorName = message.AuthorName,
                MessageId = message.MessageId,
                CreatedAt = message.CreatedAt,
                AdditionalProperties = message.AdditionalProperties
            });
        }

        return sanitized;
    }

    private static bool IsRelevant(AIContent content) => content switch
    {
        TextReasoningContent => false,
        TextContent text => !string.IsNullOrWhiteSpace(text.Text),
        _ => true
    };

    private void LogContextSize(string stage, Guid sessionId, List<ChatMessage> messages)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        var characters = messages.SelectMany(m => m.Contents).Sum(EstimateLength);

        _logger.LogDebug("Contexto [{Stage}] de la sesión {SessionId}: {Messages} mensajes, ~{Tokens} tokens estimados",
            stage, sessionId, messages.Count, characters / 4);
    }

    private static int EstimateLength(AIContent content) => content switch
    {
        TextContent text => text.Text.Length,
        TextReasoningContent reasoning => reasoning.Text.Length,
        FunctionResultContent result => result.Result?.ToString()?.Length ?? 0,
        FunctionCallContent call => call.Name.Length + 32,
        _ => 0
    };

    private sealed record ApprovalPreparation(WorkflowSession? Session, string? Message);
}

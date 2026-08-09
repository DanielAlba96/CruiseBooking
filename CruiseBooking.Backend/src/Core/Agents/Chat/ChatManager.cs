using Core.Agents.Models;
using Core.Agents.Orchestration;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Agents.Chat;

/// <inheritdoc />
internal class ChatManager(
    IWorkflowFactory workflowFactory,
    IExtraRepository extraRepository,
    ICacheService cacheService,
    UserInfo userInfo) : IChatManager
{
    private readonly IExtraRepository _extraRepository = extraRepository;
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
                    foreach (var content in agentResponse.Update.Contents)
                    {
                        switch (content)
                        {
                            case TextContent { Text.Length: > 0 } text:
                                yield return new ChatStreamEventToken(text.Text);
                                break;

                            case ToolApprovalRequestContent approval:
                                var toolCallId = approval.ToolCall.CallId;
                                await _cacheService.SetAsync($"chat:{sessionId}:approval:{toolCallId}", approval, TimeSpan.FromMinutes(10), CancellationToken.None);
                                var summary = await BuildBookingSummary(sessionId, ct);
                                yield return new ChatStreamEventApproval(toolCallId, summary);
                                break;

                            case ErrorContent error:
                                yield return new ChatStreamEventError(error.Message);
                                break;
                        }
                    }

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

        sessionMessages.AddRange(newMessages.Skip(sessionMessages.Count));

        workflowSession = new WorkflowSession(sessionId, _userInfo.Id, sessionMessages);
        await _cacheService.SetAsync($"chat:{sessionId}:state", workflowSession,
            TimeSpan.FromMinutes(10),
            CancellationToken.None);
    }

    private async Task<string> BuildBookingSummary(Guid sessionId, CancellationToken ct)
    {
        var draft = await _cacheService.GetAsync<BookingDraft>($"chat:{sessionId}:draft", ct);

        var availableExtras = await _extraRepository.GetExtrasAsync(draft!.CruiseDateId);
        var extraPrices = availableExtras.ToDictionary(e => e.ExtraId, e => e.Price);

        decimal total = 0.0m;

        var culture = CultureInfo.GetCultureInfo("es-ES");
        var sb = new StringBuilder();

        sb.AppendLine("# Resumen de su reserva");
        sb.AppendLine();

        sb.AppendLine("## Camarotes");
        sb.AppendLine();

        if (draft.Cabins.Count > 0)
        {
            sb.AppendLine("| Camarote | Nº Pasajeros | Precio |");
            sb.AppendLine("| --- | --- | --- |");

            foreach (var cabin in draft.Cabins)
            {
                total += cabin.Price;
                sb.AppendLine($"| {cabin.Name} | {cabin.Occupants} | {cabin.Price.ToString("C", culture)} |");
            }
        }
        else
        {
            sb.AppendLine("_Sin camarotes seleccionados._");
        }

        sb.AppendLine();
        sb.AppendLine("## Extras");
        sb.AppendLine();

        if (draft.Extras.Count > 0)
        {
            sb.AppendLine("| Extra | Precio |");
            sb.AppendLine("| --- | --- |");

            foreach (var extra in draft.Extras)
            {
                total += extraPrices[extra.ExtraId];
                sb.AppendLine($"| {extra.Name} | {extraPrices[extra.ExtraId].ToString("C", culture)} |");
            }
        }
        else
        {
            sb.AppendLine("_Sin extras seleccionados._");
        }

        sb.AppendLine();
        sb.AppendLine($"**Total: {total.ToString("C", culture)}**");

        return sb.ToString();
    }
}

using Carter;
using Core.Agents.Chat;
using Core.Agents.Models;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using System.Diagnostics;
using System.Net.ServerSentEvents;

namespace Core.Api.Endpoints;

/// <summary>Módulo Carter que expone el endpoint de chat interactivo con streaming.</summary>
public sealed class ChatModule : ICarterModule
{
    private const string TokenEvent = "token";
    private const string ApprovalRequiredEvent = "approval_required";
    private const string FailedEvent = "failed";

    /// <summary>Agrega las rutas de chat al constructor de endpoints.</summary>
    /// <param name="app">Constructor de rutas de endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").RequireAuthorization();

        group.MapPost("/sessions/{sessionId:Guid}", (
            Guid sessionId,
            ChatMessageDto request,
            IChatManager chatManager,
            CancellationToken ct) =>
        {
            if (request.Message.Length > 2_000)
                throw new ValidationException("El mensaje es demasiado grande");

            var events = chatManager
                .StartChatStreamAsync(sessionId, request.Message, ct)
                .Select(ToSseItem);

            return TypedResults.ServerSentEvents(events);
        });

        group.MapPost("/sessions/{sessionId:Guid}/approvals/{callId}", (
            Guid sessionId,
            string callId,
            ApprovalDecisionDto request,
            IChatManager chatManager,
            CancellationToken ct) =>
        {
            var events = chatManager
                .StartChatStreamWithApprovalAsync(sessionId, callId, request.Approved, ct)
                .Select(ToSseItem);

            return TypedResults.ServerSentEvents(events);
        });
    }

    private static SseItem<object> ToSseItem(ChatStreamEvent chatEvent) => chatEvent switch
    {
        ChatStreamEventToken t => new SseItem<object>(new { text = t.Text }, TokenEvent),
        ChatStreamEventApproval a => new SseItem<object>(new { callId = a.CallId, summary = a.Summary }, ApprovalRequiredEvent),
        ChatStreamEventError f => new SseItem<object>(new { reason = f.Reason }, FailedEvent),
        _ => throw new UnreachableException()
    };
}

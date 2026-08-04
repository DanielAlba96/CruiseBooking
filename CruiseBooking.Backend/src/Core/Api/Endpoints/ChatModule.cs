using Core.Application.Models;
using Carter;
using Shared.Domain.Services;
using System.Net.ServerSentEvents;

namespace Core.Api.Endpoints;

/// <summary>Módulo Carter que expone el endpoint de chat interactivo con streaming.</summary>
public sealed class ChatModule : ICarterModule
{
    /// <summary>Agrega las rutas de chat al constructor de endpoints.</summary>
    /// <param name="app">Constructor de rutas de endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").RequireAuthorization();

        group.MapPost("/sessions/{sessionId:Guid}", async (
            Guid sessionId,
            ChatMessageDto request,
            IChatManager chatManager,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var tokens = chatManager.StartChatStreamAsync(sessionId, request.Message, ct)
                .Select(t => new SseItem<string>(t));

            return TypedResults.ServerSentEvents(tokens, eventType: "token");
        });
    }
}

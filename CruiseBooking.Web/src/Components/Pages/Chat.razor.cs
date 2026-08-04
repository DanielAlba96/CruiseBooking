using CruiseBooking.Integrations;
using CruiseBooking.Integrations.Models;
using CruiseBooking.Vms;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Proporciona una interfaz de chat en tiempo real con un asistente de IA para asistencia en reservas de cruceros.
/// </summary>
public partial class Chat(IChatApi chatApi, ISnackbar snackbar, ILogger<Chat> logger) : IAsyncDisposable
{
    static readonly MarkdownPipeline _mdPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    readonly IChatApi _chatApi = chatApi;
    readonly ISnackbar _snackbar = snackbar;
    readonly ILogger<Chat> _logger = logger;

    Guid _sessionId;
    readonly List<ChatMessageVm> _messages = [];
    string _input = string.Empty;
    bool _isStreaming;
    CancellationTokenSource? _cts;

    sealed record ChatStreamEvent([property: JsonPropertyName("data")] string? Data,  [property: JsonPropertyName("eventType")] string? EventType);

    protected override async Task OnInitializedAsync()
    {
        _sessionId = Guid.NewGuid();
        await StreamAssistantAsync("Inicia la conversación saludando brevemente al usuario y ofreciéndole ayuda en la reserva de cruceros");
    }

    async Task OnInputKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await SendAsync();
    }

    async Task SendAsync()
    {
        var text = _input?.Trim();
        if (string.IsNullOrEmpty(text) || _isStreaming)
            return;

        _messages.Add(new ChatMessageVm { IsUser = true, Content = text });
        _input = string.Empty;

        await StreamAssistantAsync(text);
    }

    async Task StreamAssistantAsync(string triggerMessage)
    {
        var assistant = new ChatMessageVm { IsUser = false, IsStreaming = true };
        _messages.Add(assistant);
        _isStreaming = true;
        _cts = new CancellationTokenSource();

        var received = 0;
        try
        {
            await foreach (var delta in GetChatStreamAsync(_sessionId, triggerMessage, _cts.Token))
            {
                received++;
                assistant.Content += delta;
                await InvokeAsync(StateHasChanged);
            }

            if (received == 0)
            {
                _logger.LogWarning("El stream de chat se cerró sin tokens (sesión {SessionId})", _sessionId);
                assistant.Content = "(sin respuesta)";
                _snackbar.Add("El asistente no respondió. Inténtalo de nuevo.", Severity.Warning);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelado al navegar fuera de la página: sin acción.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el stream de chat (sesión {SessionId})", _sessionId);
            _snackbar.Add("Error en el chat. Inténtalo de nuevo.", Severity.Error);
        }
        finally
        {
            assistant.IsStreaming = false;
            _isStreaming = false;
            _cts?.Dispose();
            _cts = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    async IAsyncEnumerable<string> GetChatStreamAsync(
        Guid sessionId,
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var stream = await _chatApi.StreamChat(
            sessionId, new ChatMessageRequest(message), cancellationToken);

        await foreach (SseItem<string> item in SseParser.Create(stream).EnumerateAsync(cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(item.Data))
                continue;

            var chunk = JsonSerializer.Deserialize<ChatStreamEvent>(item.Data, JsonOptions);
            if (!string.IsNullOrEmpty(chunk?.Data))
                yield return chunk.Data;
        }
    }

    static MarkupString RenderMarkdown(string? content) =>
        new(Markdown.ToHtml(content ?? string.Empty, _mdPipeline));

    static string BubbleStyle(ChatMessageVm msg) => msg.IsUser
        ? "max-width: 75%; background: var(--mud-palette-primary); color: var(--mud-palette-primary-text);"
        : "max-width: 75%; background: var(--mud-palette-background-grey);";

    /// <summary>
    /// Cleans up the chat session when the component is disposed.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        return ValueTask.CompletedTask;
    }
}

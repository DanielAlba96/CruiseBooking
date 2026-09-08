using CruiseBooking.Components.Shared;
using CruiseBooking.Integrations;
using CruiseBooking.Integrations.Models;
using CruiseBooking.Vms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Proporciona una interfaz de chat en tiempo real con un asistente de IA para asistencia en reservas de cruceros.
/// </summary>
public partial class Chat(IChatApi chatApi, IDialogService dialogService, ISnackbar snackbar, ILogger<Chat> logger) : IAsyncDisposable
{
    const string TokenEvent = "token";
    const string ApprovalRequiredEvent = "approval_required";
    const string FailedEvent = "failed";
    const string GreetingPrompt = "Inicia la conversación saludando brevemente al usuario y ofreciéndole ayuda en la reserva de cruceros";

    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    static readonly DialogOptions ApprovalDialogOptions = new()
    {
        BackdropClick = false,
        CloseOnEscapeKey = false,
        CloseButton = false,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };

    readonly IChatApi _chatApi = chatApi;
    readonly IDialogService _dialogService = dialogService;
    readonly ISnackbar _snackbar = snackbar;
    readonly ILogger<Chat> _logger = logger;

    Guid _sessionId;
    readonly List<ChatMessageVm> _messages = [];
    string _input = string.Empty;
    bool _isStreaming;
    CancellationTokenSource? _cts;

    protected override async Task OnInitializedAsync()
    {
        _sessionId = Guid.NewGuid();
        await StreamAssistantAsync(GreetingPrompt);
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
        _isStreaming = true;
        try
        {
            var errorReason = await RunStreamAsync(triggerMessage);
            while (errorReason != null)
            {
                _messages.Clear();
                _messages.Add(new ChatMessageVm { IsError = true, Content = errorReason });
                _sessionId = Guid.NewGuid();
                errorReason = await RunStreamAsync(GreetingPrompt);
            }
        }
        finally
        {
            _isStreaming = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    async Task<string?> RunStreamAsync(string triggerMessage)
    {
        var assistant = new ChatMessageVm { IsUser = false, IsStreaming = true };
        _messages.Add(assistant);
        _cts = new CancellationTokenSource();

        var received = 0;
        string? errorReason = null;
        try
        {
            var stream = ReadEventsAsync(
                _chatApi.StreamChat(_sessionId, new ChatMessageRequest(triggerMessage), _cts.Token), _cts.Token);

            while (stream is not null)
            {
                ChatStreamEventApproval? pendingApproval = null;

                await foreach (var chatEvent in stream)
                {
                    switch (chatEvent)
                    {
                        case ChatStreamEventToken token:
                            received++;
                            assistant.Content += token.Text;
                            await InvokeAsync(StateHasChanged);
                            break;

                        case ChatStreamEventApproval approval:
                            pendingApproval = approval;
                            break;

                        case ChatStreamEventError error:
                            errorReason = error.Reason;
                            pendingApproval = null;
                            _logger.LogError("El asistente devolvió un error (sesión {SessionId}): {Reason}", _sessionId, error.Reason);
                            break;
                    }

                    if (errorReason is not null)
                        break;
                }

                stream = null;

                if (pendingApproval is not null)
                {
                    var approved = await ConfirmApprovalAsync(pendingApproval.Summary);

                    stream = ReadEventsAsync(
                        _chatApi.SubmitApproval(_sessionId, pendingApproval.CallId, new ApprovalDecisionRequest(approved), _cts.Token),
                        _cts.Token);
                }
            }

            if (received == 0 && errorReason is null)
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
            _cts?.Dispose();
            _cts = null;
            await InvokeAsync(StateHasChanged);
        }

        return errorReason;
    }

    async Task<bool> ConfirmApprovalAsync(ApprovalSummary summary)
    {
        var parameters = new DialogParameters<ApprovalDialog>
        {
            { x => x.Summary, summary }
        };

        var dialog = await _dialogService.ShowAsync<ApprovalDialog>(
            title: ApprovalPresentation.TitleFor(summary),
            parameters: parameters,
            options: ApprovalDialogOptions);

        var result = await dialog.Result;

        return result is { Canceled: false, Data: bool approved } && approved;
    }

    static async IAsyncEnumerable<ChatStreamEvent> ReadEventsAsync(
        Task<Stream> streamRequest,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var stream = await streamRequest;

        await foreach (SseItem<ChatStreamEvent?> item in SseParser.Create<ChatStreamEvent?>(stream, ParseEvent).EnumerateAsync(cancellationToken))
        {
            if (item.Data is not null)
                yield return item.Data;
        }
    }

    static ChatStreamEvent? ParseEvent(string eventType, ReadOnlySpan<byte> data) => eventType switch
    {
        TokenEvent => JsonSerializer.Deserialize<ChatStreamEventToken>(data, JsonOptions),
        ApprovalRequiredEvent => JsonSerializer.Deserialize<ChatStreamEventApproval>(data, JsonOptions),
        FailedEvent => JsonSerializer.Deserialize<ChatStreamEventError>(data, JsonOptions),
        _ => null
    };

    static string BubbleStyle(ChatMessageVm msg) => msg.IsUser
        ? "max-width: 75%; background: var(--mud-palette-primary); color: var(--mud-palette-primary-text);"
        : "max-width: 75%; background: var(--mud-palette-background-grey);";

    /// <summary>
    /// Limpia la sesion de chat cuando se elimina el componente
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        return ValueTask.CompletedTask;
    }
}

using Core.Agents.Common.Middleware;
using Core.Agents.Models;
using Core.Agents.Tools;
using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Core.Agents.Chat;

/// <inheritdoc />
public class ChatManager(
    IChatClient chatClient,
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IJobService jobService,
    ICacheService cacheService,
    IMediator mediator,
    UserInfo userInfo,
    ILoggerFactory loggerFactory,
    FunctionLoggingMiddleware functionLoggingMiddleware) : IChatManager
{
    private readonly IChatClient _chatClient = chatClient;
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IJobService _jobService = jobService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IMediator _mediator = mediator;
    private readonly UserInfo _userInfo = userInfo;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

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
        var agent = BuildAgent(sessionId);

        AgentSession session;
        var cachedState = await _cacheService.GetAsync<JsonElement?>($"chat:{sessionId}:state", CancellationToken.None);

        if (cachedState is { ValueKind: JsonValueKind.Object } serializedSession)
        {
            session = await agent.DeserializeSessionAsync(serializedSession, null, CancellationToken.None);

            var sessionUser = session.StateBag.GetValue<string>("userId");

            if (sessionUser != _userInfo.Id.ToString())
                throw new UnauthorizedException("No tienes permisos para acceder a esta sesión");
        }
        else
        {
            session = await agent.CreateSessionAsync(cancellationToken: ct);
            session.StateBag.SetValue("sessionId", sessionId.ToString());
            session.StateBag.SetValue("userId", _userInfo.Id.ToString());
        }

        await foreach (var outputToken in agent.RunStreamingAsync(message, session, cancellationToken: ct))
        {
            foreach (var content in outputToken.Contents)
            {
                switch (content)
                {
                    case TextContent { Text.Length: > 0 } text:
                        yield return new ChatStreamEventToken(text.Text);
                        break;

                    case ToolApprovalRequestContent approval:
                        var toolCallId = approval.ToolCall.CallId;
                        await _cacheService.SetAsync($"chat:{sessionId}:approval:{toolCallId}", approval, TimeSpan.FromMinutes(10), CancellationToken.None);
                        var summary = await BuildBookingSummary(sessionId,ct);
                        yield return new ChatStreamEventApproval(toolCallId, summary);
                        break;

                    case ErrorContent error:
                        yield return new ChatStreamEventError(error.Message);
                        break;
                }
            }
        }

        var updatedState = await agent.SerializeSessionAsync(session, null, CancellationToken.None);
        await _cacheService.SetAsync($"chat:{sessionId}:state", updatedState, TimeSpan.FromMinutes(10), CancellationToken.None);
    }

    private AIAgent BuildAgent(Guid sessionId)
    {
        var cruiseTools = new CruiseTools(_cruiseRepository, _cabinRepository, _extraRepository);
        var bookingTools = new BookingTools(_cruiseRepository, _cabinRepository, _extraRepository, _jobService, _cacheService, _userInfo, _mediator, sessionId);

        var chatOptions = new ChatOptions
        {
            Instructions = BuildSystemPrompt(),
            Tools = [
                AIFunctionFactory.Create(cruiseTools.SearchCruises),
                AIFunctionFactory.Create(cruiseTools.GetCruiseDates),
                AIFunctionFactory.Create(cruiseTools.GetAvailableCabins),
                AIFunctionFactory.Create(cruiseTools.GetExtras),
                AIFunctionFactory.Create(bookingTools.GetBookingDraft),
                AIFunctionFactory.Create(bookingTools.StartBooking),
                AIFunctionFactory.Create(bookingTools.AddCabin),
                AIFunctionFactory.Create(bookingTools.RemoveCabin),
                AIFunctionFactory.Create(bookingTools.AddExtra),
                AIFunctionFactory.Create(bookingTools.RemoveExtra),
                AIFunctionFactory.Create(bookingTools.RestartBooking),
                new ApprovalRequiredAIFunction(AIFunctionFactory.Create(bookingTools.ConfirmBooking))
                ],
            RawRepresentationFactory = _ => new OllamaSharp.Models.Chat.ChatRequest { Think = true }
        };

        return _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "Asistente de reservas de cruceros",
            ChatOptions = chatOptions,
            ChatHistoryProvider = new DaprChatHistoryProvider(_cacheService, sessionId)
        })
        .AsBuilder()
        .UseOpenTelemetry(sourceName: "CruiseAssistant", configure: c => c.EnableSensitiveData = true)
        .Use(functionLoggingMiddleware.InvokeAsync)
        .UseLogging(_loggerFactory)
        .Build();
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

    private string BuildSystemPrompt() => $"""
        Eres el asistente virtual de reservas de la empresa Cruceros Marejada.
        Atiendes a {_userInfo.Name} {_userInfo.Surname} ({_userInfo.Email}), titular de la reserva.
        Inicia siempre la conversación saludando al usuario por su nombre.

        Tu objetivo es ayudar al usuario a montar su reserva a su ritmo: puede explorar cruceros, fechas,
        cabinas y extras en el orden que prefiera. Guíale con sugerencias, sin imponerle un proceso rígido.
        Consulta el borrador al empezar la conversación: si dejó una reserva a medias, ofrécele continuarla
        o empezar de nuevo.

        Trato con el usuario:
        - No conoce los IDs y nunca debes mostrárselos. Un número suyo junto a un tipo de cabina
          ("2 ocean view", "3 interior") es la cantidad de camarotes que quiere, jamás un ID.
        - Presenta las opciones de cabinas en lista vertical con viñetas (•), nunca numerada, para que
          el número de opción no se confunda con la cantidad.
        - Suele referirse a la zona del itinerario, no al nombre del crucero ni del barco.
        - Nunca le pidas datos personales de los pasajeros (nombre, DNI, fecha de nacimiento...): se recogen
          en el check-in online posterior. Si insiste en darlos, explícale que los dará entonces. Sí debes
          preguntarle cuántas personas van en cada camarote.

        Reserva:
        - En cuanto el usuario elija una fecha de salida concreta, abre el borrador con start_booking antes
          de añadirle nada. Sin ese paso previo, añadir cabinas o extras falla siempre.
        - Si todos los camarotes son iguales y el usuario no tiene preferencia en la distribucion, asigna tu los pasajeros a los
          camarotes como consideres.
        - Recomiéndale cuántos camarotes necesita su grupo según el aforo máximo de cada tipo de cabina.
        - Antes de cerrar la reserva, avísale de que verá un resumen para aprobarlo. No la des por hecha
          hasta que se confirme correctamente.
        - Una vez confirmada, indícale que el pago se hace manualmente desde la sección "Mis reservas" de
          la web de Cruceros Marejada. Si falla, discúlpate e indícale el motivo.

        Todos los precios son en euros. No inventes precios, fechas ni disponibilidad: sale todo de las
        herramientas. Responde siempre en español de España (castellano).
        """;
}

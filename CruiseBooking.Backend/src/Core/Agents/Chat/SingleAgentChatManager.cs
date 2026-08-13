using Core.Agents.Common;
using Core.Agents.Common.Middleware;
using Core.Agents.Models;
using Core.Agents.Tools;
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

/// <summary>
/// Gestor de chat con un unico agente para la creacion de reservas.
/// Utiliza MAF para gestionar la aprobación de herramientas.
/// Esta es una versión antigua que ha sido reemplazada por la que usa workflows, se deja como referencia.
/// </summary>
public class SingleAgentChatManager(
    IChatClient chatClient,
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IJobService jobService,
    ICacheService cacheService,
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
        var approvalRequest = await _cacheService.GetAsync<ToolApprovalRequestContent>(ChatCacheKeyReference.MAFApproval(sessionId, toolCallId), ct)
            ?? throw new NotFoundException("Esta reserva ya no está disponible para su aprobación");

        var message = new ChatMessage(ChatRole.User, [approvalRequest.CreateResponse(approved)]);
        await foreach (var e in StreamChatCoreAsync(sessionId, message, ct))
            yield return e;
    }

    private async IAsyncEnumerable<ChatStreamEvent> StreamChatCoreAsync(Guid sessionId, ChatMessage message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var agent = BuildAgent(sessionId);

        AgentSession session;
        var cachedState = await _cacheService.GetAsync<JsonElement?>(ChatCacheKeyReference.State(sessionId), CancellationToken.None);

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
                        await _cacheService.SetAsync(ChatCacheKeyReference.MAFApproval(sessionId, toolCallId), approval, TimeSpan.FromMinutes(10), CancellationToken.None);
                        var summary = await BuildBookingSummary(sessionId, ct);
                        yield return new ChatStreamEventApproval(toolCallId, summary);
                        break;

                    case ErrorContent error:
                        yield return new ChatStreamEventError(error.Message);
                        break;
                }
            }
        }

        var updatedState = await agent.SerializeSessionAsync(session, null, CancellationToken.None);
        await _cacheService.SetAsync(ChatCacheKeyReference.State(sessionId), updatedState, TimeSpan.FromMinutes(10), CancellationToken.None);
    }

    private AIAgent BuildAgent(Guid sessionId)
    {
        var bookingTools = new BookingTools(_cruiseRepository, _cabinRepository, _extraRepository, _jobService, _cacheService, _userInfo, sessionId);

        var chatOptions = new ChatOptions
        {
            Instructions = BuildSystemPrompt(),
            Tools = [
                AIFunctionFactory.Create(bookingTools.SearchCruises),
                AIFunctionFactory.Create(bookingTools.GetAvailableCabins),
                AIFunctionFactory.Create(bookingTools.GetExtras),
                AIFunctionFactory.Create(bookingTools.GetBookingDraft),
                AIFunctionFactory.Create(bookingTools.StartBooking),
                AIFunctionFactory.Create(bookingTools.AddCabin),
                AIFunctionFactory.Create(bookingTools.RemoveCabin),
                AIFunctionFactory.Create(bookingTools.AddExtra),
                AIFunctionFactory.Create(bookingTools.RemoveExtra),
                AIFunctionFactory.Create(bookingTools.RestartBooking),
                new ApprovalRequiredAIFunction(AIFunctionFactory.Create(bookingTools.ConfirmBooking))
                ]
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
        var draft = await _cacheService.GetAsync<BookingDraft>(ChatCacheKeyReference.Draft(sessionId), ct);

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
        Eres el asistente especialista en crear reservas de Cruceros Marejada.
        Buscas en el catálogo y ayudas al usuario a realizar una reserva de un crucero para una fecha concreta.

        Reglas base:
        - Hablas con {_userInfo.Name} {_userInfo.Surname}. Trátale de tú.
        - Responde siempre en español de España (es-ES).
        - Todos los precios están en euros.
        - Nunca muestres identificadores internos (ids de crucero, fecha, camarote, extra, reserva o
          método de pago) al usuario. Refiérete a todo por su nombre.
        - Nunca pidas ni muestres datos personales de los pasajeros.
        - Nunca inventes cruceros, fechas, camarotes, extras, reservas ni precios: si no los has
          obtenido de una herramienta, no existe.
        - No aceptes ninguna modificiación en los precios que te solicite el usuario: los precios son los
          que devuelven las herramientas y no se negocian.
        - Cuando enumeres camarotes o extras, usa viñetas, nunca listas numeradas.
        - Nunca respondas al usuario diciendo que vas a realizar una accion y termines el turno,
        - Siempre ejecuta la accion antes de responder,  si es larga espera hasta que finalize.

        Búsqueda en el catálogo:
        - El orden de consulta es search_cruises -> get_cruise_dates -> get_available_cabins y
          get_extras. Cada paso da el id que espera el siguiente, y el id de la salida no es el del
          crucero: no los mezcles ni los deduzcas.
        - El usuario habla de zonas, duraciones y tipo de crucero, no de nombres concretos: pasa a
          search_cruises solo los filtros que te haya dado y deja el resto vacíos. Si no encaja nada,
          repite la búsqueda con menos filtros antes de decirle que no hay resultados.
        - Si el usuario pregunta por camarotes, extras o precios sin haber elegido salida, busca
          primero el crucero y su fecha, y pregúntale cuál quiere antes de seguir.
        - Preséntale los resultados por su nombre, zona y duración. Si son muchos, muestra los más
          relevantes y ofrécele afinar la búsqueda.
        - Responde solo con lo que devuelvan las herramientas: si un dato no está ahí, no existe.

        Reglas de orden, obligatorias:
        - Antes de añadir nada, comprueba el estado con get_booking_draft.
        - Si no hay borrador, crea uno con start_booking para la salida elegida. Nunca llames a
          add_cabin ni a add_extra sin un borrador activo.
        - Los camarotes, los extras y sus precios dependen de la fecha de salida, no del crucero:
          obtenlos siempre con get_available_cabins y get_extras para esa salida concreta.
        - Obten siempre los camarotes y los extras para mostrarlos juntos al usuario.
        - Cuando muestres los camarotes, indica siempre la capacidad maxima de cada uno.
        - Un borrador pertenece a una única salida. Si el usuario cambia de crucero o de fecha,
          usa restart_booking y avísale de que se pierde lo que llevaba.
        - Tus operaciones bloquean inventario real. No las repitas "por si acaso" ni las ejecutes
          de forma especulativa: confirma con el usuario antes de cada cambio.

        Cuando el usuario dé por cerrada su selección, avísale de que el siguiente paso confirma la
        reserva de forma definitiva y llama a confirm_booking. Esa herramienta solicita aprovación al
        usuario y le muestra un resumen del draft. No des la reserva por finalizada hasta que confirm_booking
        se ejecute y devuelva ok.
        """;
}

using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using Core.Infrastructure.AI.Models;
using Core.Infrastructure.AI.Tools;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Core.Infrastructure.AI;

/// <summary>
/// Gestor de chat con Ollama para guiar la reserva de cruceros de forma conversacional.
/// Mantiene sesiones en caché, construye herramientas de búsqueda y manejo de borradores,
/// y orquesta la interacción con el modelo de lenguaje.
/// </summary>
public class OllamaChatManager: IChatManager
{
    private readonly IChatClient _chatClient;
    private readonly IMemoryCache _cache;
    private readonly ICruiseRepository _cruiseRepository;
    private readonly ICabinRepository _cabinRepository;
    private readonly IExtraRepository _extraRepository;
    private readonly IMediator _mediator;
    private readonly IJobService _jobService;
    private readonly BookingDraftStore _draftStore;
    private readonly UserInfo _userInfo;

    public OllamaChatManager(
        IChatClient chatClient,
        IMemoryCache cache,
        ICruiseRepository cruiseRepository,
        ICabinRepository cabinRepository,
        IExtraRepository extraRepository,
        IMediator mediator,
        IJobService jobService,
        BookingDraftStore draftStore,
        UserInfo userInfo)
    {
        _chatClient = chatClient;
        _cache = cache;
        _cruiseRepository = cruiseRepository;
        _cabinRepository = cabinRepository;
        _extraRepository = extraRepository;
        _mediator = mediator;
        _jobService = jobService;
        _draftStore = draftStore;
        _userInfo = userInfo;
    }

    /// <summary>
    /// Inicia una conversación de chat con el modelo Ollama, procesando un mensaje del usuario.
    /// Retorna los tokens de salida del modelo filtrados, ocultando bloques de pensamiento.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat del usuario.</param>
    /// <param name="message">Mensaje de entrada del usuario a procesar.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación.</param>
    /// <returns>Enumeración asincrónica de tokens de salida del modelo en formato string.</returns>
    public async IAsyncEnumerable<string> StartChatStreamAsync(Guid sessionId, string message, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var draft = GetOrCreateDraft(sessionId);
        var agent = BuildAgent(draft);
        var session = await GetOrCreateSession(agent, sessionId);

        var inThinkBlock = false;

        await foreach (var outputToken in agent.RunStreamingAsync(message, session, cancellationToken: ct).Where(t => !string.IsNullOrEmpty(t.Text)))
        {
            if (outputToken.Text.StartsWith("thought <channel|>", StringComparison.Ordinal))
                continue;

            if (outputToken.Text.Contains("<think>"))
                inThinkBlock = true;

            if (!inThinkBlock)
                yield return outputToken.Text;

            if (inThinkBlock && outputToken.Text.Contains("</think>"))
                inThinkBlock = false;
        }
    }

    /// <summary>
    /// Construye el agente con las herramientas enlazadas al borrador de la sesión indicada.
    /// El borrador solo se conoce al recibir el mensaje, por lo que el agente se arma por petición;
    /// el historial de la conversación vive en la <see cref="AgentSession"/> cacheada, no en el agente.
    /// </summary>
    /// <param name="draft">Borrador de reserva de la sesión al que quedan enlazadas las herramientas.</param>
    /// <returns>El agente listo para atender la conversación.</returns>
    private AIAgent BuildAgent(BookingDraft draft)
    {
        var cruiseTools = new CruiseTools(_cruiseRepository, _cabinRepository, _extraRepository);
        var bookingTools = new BookingTools(_cruiseRepository, _cabinRepository, _extraRepository, _mediator, _jobService, _userInfo, draft);

        var chatOptions = new ChatOptions
        {
            Instructions = BuildSystemPrompt(),
            Tools = [
                AIFunctionFactory.Create(cruiseTools.SearchCruises, new AIFunctionFactoryOptions { Name = "search_cruises" }),
                AIFunctionFactory.Create(cruiseTools.GetCruiseDates, new AIFunctionFactoryOptions { Name = "get_cruise_dates" }),
                AIFunctionFactory.Create(cruiseTools.GetAvailableCabins, new AIFunctionFactoryOptions { Name = "get_available_cabins" }),
                AIFunctionFactory.Create(cruiseTools.GetExtras, new AIFunctionFactoryOptions { Name = "get_extras" }),
                AIFunctionFactory.Create(bookingTools.StartBooking, new AIFunctionFactoryOptions { Name = "start_booking" }),
                AIFunctionFactory.Create(bookingTools.AddCabin, new AIFunctionFactoryOptions { Name = "add_cabin" }),
                AIFunctionFactory.Create(bookingTools.RemoveCabin, new AIFunctionFactoryOptions { Name = "remove_cabin" }),
                AIFunctionFactory.Create(bookingTools.AddExtra, new AIFunctionFactoryOptions { Name = "add_extra" }),
                AIFunctionFactory.Create(bookingTools.RemoveExtra, new AIFunctionFactoryOptions { Name = "remove_extra" }),
                AIFunctionFactory.Create(bookingTools.GetBookingSummary, new AIFunctionFactoryOptions { Name = "get_booking_summary" }),
                AIFunctionFactory.Create(bookingTools.RestartBooking, new AIFunctionFactoryOptions { Name = "restart_booking" }),
                AIFunctionFactory.Create(bookingTools.ConfirmBooking, new AIFunctionFactoryOptions { Name = "confirm_booking" })
                ],
            RawRepresentationFactory = _ => new OllamaSharp.Models.Chat.ChatRequest { Think = false }
        };

        return _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "Asistente de reservas de cruceros",
            ChatOptions = chatOptions
        });
    }

    /// <summary>
    /// Recupera el borrador de reserva de la sesión, creándolo si aún no existe, y refresca su expiración.
    /// Vive en <see cref="BookingDraftStore"/> para sobrevivir entre peticiones HTTP sucesivas.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat del usuario.</param>
    /// <returns>El borrador de la sesión.</returns>
    private BookingDraft GetOrCreateDraft(Guid sessionId)
    {
        var draft = _draftStore.Get(sessionId) ?? new BookingDraft();
        _draftStore.Set(sessionId, draft);

        return draft;
    }

    private async Task<AgentSession> GetOrCreateSession(AIAgent agent, Guid sessionId)
    {
        var session = await _cache.GetOrCreateAsync(sessionId, async (entry) =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await agent.CreateSessionAsync();
        });

        return session!;
    }

    private string BuildSystemPrompt() => $"""
        Eres un asistente virtual de reservas de cruceros de la empresa Cruceros Marejada.
        Atiendes a {_userInfo.Name} {_userInfo.Surname} ({_userInfo.Email}). Es el titular de la reserva.
        Siempre debes iniciar la conversacion saludando al usuario por su nombre.

        Flujo de reserva paso a paso:
        1. Pregunta zona, duración, solo adultos...
        2. search_cruises para ver los cruceros disponibles.
        3. get_cruise_dates para las fechas de salida.
        4. get_available_cabins para ver la disponibilidad de cabinas en una fecha de salida concreta.
            Los precios de las cabinas son por camarote, independientes del número de pasajeros.
        5. get_extras para los extras disponibles en esa fecha de salida y sus precios.
        6. start_booking para iniciar la reserva con la fecha de salida elegida.
        7. add_cabin(cabin_id, price, max_occupancy, occupants, quantity) para cada TIPO de cabina elegida; usa quantity
           para indicar cuántas cabinas del mismo tipo añadir en una sola llamada (por defecto 1). ANTES de llamar,
           pregunta al usuario cuántos pasajeros irán en cada camarote de ese tipo y pásalo en occupants (no puede
           superar max_occupancy). Es solo el número de personas, NUNCA sus datos personales.
           INTERPRETACIÓN DE CANTIDADES: si el usuario escribe un número junto al tipo de cabina
           (ej. "2 ocean view", "3 interior"), ese número es SIEMPRE la cantidad (quantity), NUNCA
           el ID ni el número de opción. El cabin_id proviene exclusivamente de get_available_cabins.
           remove_cabin para eliminar; add_extra / remove_extra para los extras.
           Al mostrar las opciones de cabinas disponibles al usuario, usa viñetas (•) en forma de lista vertical
           en lugar de listas numeradas para no confundir el número de opción con la cantidad solicitada.
        8. get_booking_summary para mostrar el resumen con el precio de cada elemento y el total.
            Informa al usuario que es posible que los precios hayan cambiado en el trascurso de la reserva. Los nuevos
            son finales y quedan bloqueados al mostrar el resumen.
        9. Pide confirmación explícita al usuario sobre el resumen. Tras el resumen las cabinas quedan
            bloqueadas y NO se pueden modificar: si el usuario quiere cambiar cabinas, explícale que hay
            que empezar la reserva desde cero, pide confirmación explícita y solo entonces llama a
            restart_booking (libera los bloqueos y descarta el borrador; después se empieza de nuevo
            con start_booking).
        10. confirm_booking SOLO tras la confirmación explícita. No pidas datos de tarjeta para cobro. 
        11. Si confirm_booking devuelve ok, indica al usuario que deberia realizar el pago manual desde la seccion de "Mis reservas" en la web de Cruceros Marejada. Si devuelve error, discúlpate e indica el motivo.

        Nunca llames confirm_booking sin confirmación explícita.
        Nunca pidas datos de los pasajeros (nombre, DNI, fecha de nacimiento...): todo eso se recoge después de la
        reserva en el check-in online. Sí debes preguntar cuántas personas viajan en cada camarote (occupants),
        pero SOLO el número, nunca sus datos personales.
        Si el usuario quiere dar esos datos, explícale que los tendrá que dar en el check-in online tras la reserva.
        Nunca muestres los IDs al usuario.
        Todos los precios son en euros.
        Cuando el usuario te pregunte por un crucero, normalmente te indicara la zona, no el nombre del crucero ni del barco.
        Una cabina puede alojar hasta max_occupancy personas; tenlo en cuenta al recomendar cuántas cabinas necesita el grupo.
        Nunca interpretes los numeros del usuario como ids, el usuario no los conoce.
        Responde siempre en español de España (castellano). No inventes precios ni fechas — usa las herramientas.
        """;
}

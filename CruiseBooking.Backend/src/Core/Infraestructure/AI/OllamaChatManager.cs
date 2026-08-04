using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using Core.Infrastructure.AI.Models;
using Core.Infrastructure.AI.Tools;
using Microsoft.Extensions.Caching.Memory;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace Core.Infrastructure.AI;

/// <summary>
/// Gestor de chat con Ollama para guiar la reserva de cruceros de forma conversacional.
/// Mantiene sesiones en caché, construye herramientas de búsqueda y manejo de borradores,
/// y orquesta la interacción con el modelo de lenguaje.
/// </summary>
public class OllamaChatManager(
    IOllamaApiClient ollamaClient,
    IMemoryCache cache,
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IMediator mediator,
    IJobService jobService,
    BookingDraftStore draftStore,
    UserInfo userInfo)
    : IChatManager
{
    private readonly IOllamaApiClient _ollamaClient = ollamaClient;
    private readonly IMemoryCache _cache = cache;
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IMediator _mediator = mediator;
    private readonly IJobService _jobService = jobService;
    private readonly BookingDraftStore _draftStore = draftStore;
    private readonly UserInfo _userInfo = userInfo;

    private readonly List<Tool> _readOnlyTools =
    [
        new SearchCruisesTool(cruiseRepository),
        new GetCruiseDatesTool(cruiseRepository),
        new GetAvailableCabinsTool(cabinRepository),
        new GetExtrasTool(extraRepository)
    ];

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
        var chat = GetOrCreateSession(sessionId);
        var tools = new List<Tool>(_readOnlyTools);
        tools.AddRange(BuildDraftTools(sessionId));

        var inThinkBlock = false;

        await foreach (var outputToken in chat.SendAsync(message, tools, null, cancellationToken: ct).Where(t => !string.IsNullOrEmpty(t)))
        {
            if (outputToken.StartsWith("thought <channel|>", StringComparison.Ordinal))
                continue;

            if (outputToken.Contains("<think>"))
                inThinkBlock = true;

            if (!inThinkBlock)
                yield return outputToken;

            if (inThinkBlock && outputToken.Contains("</think>"))
                inThinkBlock = false;
        }
    }

    private IEnumerable<Tool> BuildDraftTools(Guid sessionId) =>
    [
        new StartBookingTool(_cruiseRepository, _draftStore, sessionId),
        new AddCabinTool(_cabinRepository, _jobService, _userInfo, _draftStore, sessionId),
        new RemoveCabinTool(_cabinRepository, _userInfo, _draftStore, sessionId),
        new AddExtraTool(_extraRepository, _draftStore, sessionId),
        new RemoveExtraTool(_draftStore, sessionId),
        new GetBookingSummaryTool(_cabinRepository, _extraRepository, _draftStore, sessionId),
        new RestartBookingTool(_draftStore, sessionId, _cabinRepository, _userInfo),
        new ConfirmBookingTool(_draftStore, sessionId, _mediator)
    ];

    private Chat GetOrCreateSession(Guid sessionId)
    {
        return _cache.GetOrCreate(sessionId, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);

            var chat = new Chat(_ollamaClient) { Think = false, AllowRecursiveToolCalls = true };
            chat.Messages.Add(new Message(ChatRole.System, BuildSystemPrompt()));
            return chat;
        })!;
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

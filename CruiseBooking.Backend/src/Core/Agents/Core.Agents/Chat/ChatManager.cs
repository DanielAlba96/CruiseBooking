using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Core.Application.Common.CQRS;
using Core.Agents.Tools;

namespace Core.Agents.Chat;

/// <summary>
/// Gestor de chat para guiar la reserva de cruceros de forma conversacional.
/// Mantiene sesiones en caché, construye herramientas de búsqueda y manejo de borradores,
/// y orquesta la interacción con el modelo de lenguaje.
/// </summary>
public class ChatManager(
    IChatClient chatClient,
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IJobService jobService,
    ICacheService cacheService,
    IMediator mediator,
    UserInfo userInfo) : IChatManager
{
    private readonly IChatClient _chatClient = chatClient;
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IJobService _jobService = jobService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IMediator _mediator = mediator;
    private readonly UserInfo _userInfo = userInfo;

    /// <summary>
    /// Inicia una conversación de chat, procesando un mensaje del usuario.
    /// Retorna los tokens de salida del modelo filtrados, ocultando bloques de pensamiento.
    /// </summary>
    /// <param name="sessionId">Identificador único de la sesión de chat del usuario.</param>
    /// <param name="message">Mensaje de entrada del usuario a procesar.</param>
    /// <param name="ct">Token de cancelación para interrumpir la operación.</param>
    /// <returns>Enumeración asincrónica de tokens de salida del modelo en formato string.</returns>
    public async IAsyncEnumerable<string> StartChatStreamAsync(Guid sessionId, string message, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var agent = BuildAgent(sessionId);

        var session = await _cacheService.GetAsync<AgentSession>($"chat:{sessionId}:state", CancellationToken.None);
        if (session is not null)
        {
            var sessionUser = session.StateBag.GetValue<string>("userId");

            if (sessionUser != _userInfo.Id.ToString())
                throw new UnauthorizedException("La sesión de chat no pertenece al usuario actual.");
        }
        else
        {
            session = await agent.CreateSessionAsync(cancellationToken: ct);
            session.StateBag.SetValue("sessionId", sessionId.ToString());
            session.StateBag.SetValue("userId", _userInfo.Id.ToString());
        }

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

        await _cacheService.SetAsync($"chat:{sessionId}:state", session, TimeSpan.FromMinutes(10), CancellationToken.None);
    }

    private ChatClientAgent BuildAgent(Guid sessionId)
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
                AIFunctionFactory.Create(bookingTools.StartBooking),
                AIFunctionFactory.Create(bookingTools.AddCabin),
                AIFunctionFactory.Create(bookingTools.RemoveCabin),
                AIFunctionFactory.Create(bookingTools.AddExtra),
                AIFunctionFactory.Create(bookingTools.RemoveExtra),
                AIFunctionFactory.Create(bookingTools.GetBookingSummary),
                AIFunctionFactory.Create(bookingTools.RestartBooking),
                AIFunctionFactory.Create(bookingTools.ConfirmBooking)
                ],
            RawRepresentationFactory = _ => new OllamaSharp.Models.Chat.ChatRequest { Think = false }
        };

        return _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "Asistente de reservas de cruceros",
            ChatOptions = chatOptions,
            ChatHistoryProvider = new DaprChatHistoryProvider(_cacheService, sessionId)
        });
    }

    private string BuildSystemPrompt() => $"""
        Eres un asistente virtual de reservas de cruceros de la empresa Cruceros Marejada.
        Atiendes a {_userInfo.Name} {_userInfo.Surname} ({_userInfo.Email}). Es el titular de la reserva.
        Siempre debes iniciar la conversacion saludando al usuario por su nombre.

        Flujo de reserva paso a paso:
        0. Llama a la tool get_booking_summary para saber si hay una reserva en curso. Si el borrador esta vacio, empieza por el punto 1, en caso contrario, pregunta al usuario si quiere continuar con ella o empezar de nuevo. Si quiere empezar de nuevo, llama a restart_booking y empieza por el punto 1. Si quiere continuar, pregunta qué quiere cambiar (camarotes, extras, fechas...). Si quiere cambiar camarotes, ejecuta los pasos 4 y 7. Si quiere cambiar extras, ejecuta los pasos 5 y 7. Si quiere cambiar fechas llama a restart_booking y empieza de nuevo por el punto 1. Si no quiere cambiar nada, pasa al punto 9.
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
        9. Pide confirmación explícita al usuario sobre el resumen antes de proceder. 
        10. confirm_booking SOLO tras la confirmación explícita. No pidas datos de tarjeta para cobro. 
        11. Si confirm_booking devuelve ok, indica al usuario que deberia realizar el pago manual desde la seccion de "Mis reservas" en la web de Cruceros Marejada. Si devuelve error, discúlpate e indica el motivo.

        Nunca llames confirm_booking sin confirmación explícita.
        Si el usuario pide cambiar el crucero  o las fechas de salida, reinicia la reserva con restart_booking y empieza de nuevo por el punto 1.
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

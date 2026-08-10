using Core.Agents.Common;
using Core.Agents.Common.Middleware;
using Core.Agents.Tools;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Agents.Orchestration;

/// <inheritdoc />
internal sealed class AgentFactory(
    IChatClient chatClient,
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IJobService jobService,
    ICacheService cacheService,
    IMediator mediator,
    UserInfo userInfo,
    ILoggerFactory loggerFactory,
    FunctionLoggingMiddleware functionLoggingMiddleware,
    IHostEnvironment environment) : IAgentFactory
{
    private const string TelemetrySourceName = "CruiseAssistant";

    private readonly IChatClient _chatClient = chatClient;
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IJobService _jobService = jobService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IMediator _mediator = mediator;
    private readonly UserInfo _userInfo = userInfo;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly FunctionLoggingMiddleware _functionLoggingMiddleware = functionLoggingMiddleware;

    /// <inheritdoc />
    public AgentTeam CreateChatAgents(Guid sessionId)
    {
        var bookingTools = new BookingTools(_cruiseRepository,
            _cabinRepository,
            _extraRepository,
            _jobService,
            _cacheService,
            _userInfo,
            sessionId);

        var postSalesTools = new PostSalesTools(_mediator, _cacheService, sessionId);

        var triage = CreateTriageAgent();
        var booking = CreateBookingAgent(bookingTools);
        var postSales = CreatePostSalesAgent(postSalesTools);

        return new AgentTeam(triage, booking, postSales);
    }

    private AIAgent CreateTriageAgent()
    {
        ChatOptions chatOptions = new()
        {
            Instructions = BuildTriageInstructions(),
            Tools = []
        };

        return _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Id = AgentNames.Triage,
            Name = AgentNames.Triage,
            Description = "Recibe al usuario, entiende qué necesita y transfiere la conversación al especialista adecuado.",
            ChatOptions = chatOptions
        })
        .AsBuilder()
        .UseOpenTelemetry(
            sourceName: TelemetrySourceName,
            configure: c => environment.IsDevelopment())
        .Use(_functionLoggingMiddleware.InvokeAsync)
        .UseLogging(_loggerFactory)
        .Build();
    }

    private AIAgent CreateBookingAgent(BookingTools bookingTools)
    {
        ChatOptions chatOptions = new()
        {
            Instructions = BuildBookingInstructions(),
            Tools = [
                AIFunctionFactory.Create(bookingTools.SearchCruises),
                AIFunctionFactory.Create(bookingTools.GetCruiseDates),
                AIFunctionFactory.Create(bookingTools.GetAvailableCabins),
                AIFunctionFactory.Create(bookingTools.GetExtras),
                AIFunctionFactory.Create(bookingTools.GetBookingDraft),
                AIFunctionFactory.Create(bookingTools.StartBooking),
                AIFunctionFactory.Create(bookingTools.AddCabin),
                AIFunctionFactory.Create(bookingTools.RemoveCabin),
                AIFunctionFactory.Create(bookingTools.AddExtra),
                AIFunctionFactory.Create(bookingTools.RemoveExtra),
                AIFunctionFactory.Create(bookingTools.RestartBooking),
                AIFunctionFactory.Create(bookingTools.ConfirmBooking)
            ]
        };

        return _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Id = AgentNames.Booking,
            Name = AgentNames.Booking,
            Description = "Especialista en reservas nuevas: busca en el catálogo cruceros, fechas de salida, camarotes y extras, monta el borrador de reserva y lo confirma.",
            ChatOptions = chatOptions
        })
        .AsBuilder()
        .UseOpenTelemetry(
            sourceName: TelemetrySourceName,
            configure: c => environment.IsDevelopment())
        .Use(_functionLoggingMiddleware.InvokeAsync)
        .UseLogging(_loggerFactory)
        .Build();
    }

    private AIAgent CreatePostSalesAgent(PostSalesTools postSalesTools)
    {
        ChatOptions chatOptions = new()
        {
            Instructions = BuildPostSalesInstructions(),
            Tools = [
                AIFunctionFactory.Create(postSalesTools.GetMyBookings),
                AIFunctionFactory.Create(postSalesTools.GetBookingDetail),
                AIFunctionFactory.Create(postSalesTools.GetPaymentMethods),
                AIFunctionFactory.Create(postSalesTools.PayBooking),
                AIFunctionFactory.Create(postSalesTools.CancelBooking)
            ]
        };

        return _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Id = AgentNames.PostSales,
            Name = AgentNames.PostSales,
            Description = "Especialista en posventa: consulta las reservas que el usuario ya tiene, cobra manualmente las pendientes de pago y las cancela.",
            ChatOptions = chatOptions
        })
        .AsBuilder()
        .UseOpenTelemetry(
            sourceName: TelemetrySourceName,
            configure: c => environment.IsDevelopment())
        .Use(_functionLoggingMiddleware.InvokeAsync)
        .UseLogging(_loggerFactory)
        .Build();
    }

    private string BuildTriageInstructions() =>
        $"""
        Eres el punto de entrada del asistente para reservas.

        {BuildSharedRules()}

        Tu única función es entender qué necesita el usuario y transferir la conversación al
        especialista adecuado. No tienes herramientas de negocio y no debes responder tú mismo a
        preguntas sobre cruceros, camarotes, extras, precios ni reservas.

        La pregunta que decide el destino es si la reserva ya existe: crear una nueva es
        {AgentNames.Booking}, gestionar las que el usuario ya tiene es {AgentNames.PostSales}.

        Transfiere a {AgentNames.Booking} cuando el usuario:
        - busque cruceros, destinos, zonas o duraciones,
        - pregunte por fechas de salida, camarotes disponibles o extras,
        - quiera reservar un crucero,
        - quiera añadir o quitar camarotes o extras a la reserva que está preparando,
        - quiera confirmar esa reserva.

        Transfiere a {AgentNames.PostSales} cuando el usuario:
        - pregunte por sus reservas o por los detalles de una que ya tiene,
        - quiera pagar una reserva pendiente de pago,
        - quiera cancelar una reserva.

        Si no tienes clara la intención del usuario, pregúntale.

        Si el usuario te indica el resultado de la aprobacion de una herramiento, preguntale que quiere hacer a continuación.

        Si el mensaje es un saludo o algo ajeno al dominio, responde brevemente indicando que tu trabajo es
        resolver consultas relacionadas con reservas nuevas o existentes.
        """;

    private string BuildBookingInstructions() =>
        $"""
        Eres el asistente especialista en crear reservas. Buscas en el catálogo y ayudas al usuario a realizar una reserva
        de un crucero para una fecha concreta.

        {BuildSharedRules()}

        Búsqueda en el catálogo:
        - El orden de consulta es search_cruises -> get_cruise_dates -> get_available_cabins y
          get_extras. Cada paso da el id que espera el siguiente, y el id de la salida no es el del
          crucero: no los mezcles ni los deduzcas.
        - Siempre devuelve los cruceros con sus fechas de salida en un mismo turno.
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
        - No des por terminada una reserva hasta que el usuario apruebe la operación.
        """;

    private string BuildPostSalesInstructions() =>
        $"""
        Eres el especialista en posventa. Gestionas las reservas que el usuario ya tiene hechas
        y le ayudas a pagarlas o cancelarlas.

        {BuildSharedRules()}

        Reglas de orden, obligatorias:
        - Empieza siempre por get_my_bookings. Nunca supongas qué reservas tiene el usuario ni des
          por buena una reserva que no venga de esa herramienta.
        - Muestra las reservas como una lista numerada para que el usuario decida
         e incluye un desglose los camarotes.
        - El usuario se referirá a una reserva por el numero del listado que le has mostrado, no por su id:
          resuelve tú el id internamente y no se lo muestres.
        - Si varias reservas encajan con lo que dice, pregúntale cuál antes de operar.
        - Antes de pagar o cancelar, usa get_booking_detail y confírmale sobre qué reserva vas a
          actuar.

        Pago manual:
        - Solo es posible si la reserva tiene can_pay a true.
        - Llama a get_payment_methods y ofrécele únicamente las tarjetas que devuelva, con el formato
          "Visa ****4242 (caduca 05/2028) y como lista numerada para que el usuario eliga por numero".
          Nunca muestres el identificador del método de pago.
        - Tienes prohibido dar de alta métodos de pago. No pidas jamás el número de tarjeta, el CVV,
          la caducidad ni el titular. Si el usuario te los escribe por su cuenta, ignóralos y no los
          repitas: no sirven para nada aquí.
        - Si no tiene ninguna tarjeta dada de alta, dile que debe añadirla desde su perfil en la web
          y no le ofrezcas ninguna alternativa desde la conversación.
        - Con la tarjeta elegida, llama a pay_booking pasando su payment_method_id tal cual.

        Cancelación:
        - Solo es posible si la reserva tiene can_cancel a true.
        - Avísale de que es irreversible y de que, si la reserva ya estaba cobrada, se emitirá el
          reembolso y tardará unos días en aparecer en su tarjeta.
        - Si la reserva estaba cobrada y el periodo de check-in ya ha empezado, la cancelación se
          rechazará: explícaselo con el mensaje de error y no lo reintentes.

        No des por terminado un pago o una cancelación hasta que el usuario apruebe la operación.
        """;

    private string BuildSharedRules() =>
    $"""
        Reglas base:
        - Hablas con {_userInfo.Name} {_userInfo.Surname}. Trátale de tú.
        - Responde siempre en español de España (es-ES).
        - Nunca digas que vas a transferir a otro agente. El usuario no puede saber que hay varios.
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
        """;
}

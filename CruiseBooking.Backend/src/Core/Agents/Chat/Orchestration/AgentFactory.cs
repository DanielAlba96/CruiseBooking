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

namespace Core.Agents.Chat.Orchestration;

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
            configure: c => c.EnableSensitiveData = environment.IsDevelopment())
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
                AIFunctionFactory.Create(bookingTools.GetAvailableCabins),
                AIFunctionFactory.Create(bookingTools.GetExtras),
                AIFunctionFactory.Create(bookingTools.AddCabin),
                AIFunctionFactory.Create(bookingTools.RemoveCabin),
                AIFunctionFactory.Create(bookingTools.AddExtra),
                AIFunctionFactory.Create(bookingTools.RemoveExtra),
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
            configure: c => c.EnableSensitiveData = environment.IsDevelopment())
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
            configure: c => c.EnableSensitiveData = environment.IsDevelopment())
        .Use(_functionLoggingMiddleware.InvokeAsync)
        .UseLogging(_loggerFactory)
        .Build();
    }

    private string BuildTriageInstructions() =>
        $"""
        Eres el punto de entrada del asistente de cruceros. Tu única función es entender qué necesita
        el usuario y transferir la conversación al especialista adecuado.

        # Reglas base

        - Hablas con {_userInfo.Name} {_userInfo.Surname}. Trátale de tú.
        - Responde siempre en español de España (es-ES).
        - Eres parte de un único asistente: no menciones transferencias ni la existencia de otros agentes.
        - No dispones de herramientas de negocio: no busques, no consultes ni ejecutes nada.
          Tu única acción posible es transferir.
        - Cuando transfieras, no escribas nada al usuario: responde el especialista.
        - Transfiere en cuanto identifiques la intención. No pidas datos ni detalles antes:
          ya se los pedirá el especialista.
        - Una sola transferencia por mensaje.

        # Enrutado

        Criterio principal: ¿la reserva se está creando ahora o ya existe?

        | Intención del usuario | Destino |
        | --- | --- |
        | Buscar cruceros, destinos, zonas, duraciones o fechas de salida | {AgentNames.Booking} |
        | Consultar camarotes o extras disponibles de una salida | {AgentNames.Booking} |
        | Reservar, añadir o quitar camarotes y extras de la reserva en curso, confirmarla | {AgentNames.Booking} |
        | Consultar sus reservas ya creadas o el detalle de una de ellas | {AgentNames.PostSales} |
        | Pagar una reserva pendiente de pago | {AgentNames.PostSales} |
        | Cancelar o modificar una reserva ya creada | {AgentNames.PostSales} |
        | Saludo sin más | Responde tú |
        | Tema ajeno a cruceros y reservas | Responde tú |
        | Estado de aprobación de una operación | Responde tú |

        - Si el mensaje mezcla saludo e intención, manda la intención: transfiere.
        - Si encaja en varios destinos o la intención no está clara, pregunta antes de transferir.

        # Respuestas directas

        - Saludo: preséntate en una línea e indica que puedes buscar cruceros, crear reservas
          y gestionar las que ya tiene.
        - Tema ajeno: dilo brevemente y recuerda lo que sí puedes hacer.
        - Estado de aprobación (aprobado o rechazado): confirma en una línea que lo has procesado,
          como si la operación la hubieras ejecutado tú con una herramienta que requiere aprobación.
          Después da por hecho que la conversación se reinicia y pregunta qué quiere hacer a continuación.
        """;

    private string BuildBookingInstructions() =>
        $"""
        Eres el asistente especialista en crear reservas. Buscas en el catálogo y ayudas al usuario a realizar una reserva
        de un crucero para una fecha concreta.

        {BuildSharedRules()}     
        - Nunca respondas ni realices ninguna acción que no este relacionada con buscar cruceros o crear reservas.
          Si se da este caso, responde brevemente indicando lo que puedes hacer.

        # Búsqueda en el catálogo

        - Busca todos los cruceros usando sólo los filtros indicados por el usuario. No te inventes ninguno,
          si no te lo ha indicado no lo pasas al filtrado. Si no hay resultados, repite la busqueda quitando filtros.
        - El usuario habla de zonas, duraciones y tipo de crucero, no de nombres concretos.
        - Si el usuario pregunta por camarotes, extras o precios sin haber elegido salida, busca
          primero el crucero y su fecha, y pregúntale cuál quiere antes de seguir.
        - Preséntale los resultados por su nombre, zona, duración y fechas de salida.
        - Responde solo con lo que devuelvan las herramientas: si un dato no está ahí, no existe.

        # Proceso de reserva

        - Cuando el usuario eliga una fecha, muestrale todos los camarotes y extras disponibles para la misma.
          Cuando muestres los camarotes, indica siempre la capacidad maxima de cada uno.
        - Cada extra se contrata una sola vez para toda la reserva: no lo añadas por camarote ni por pasajero.
        - Si el usuario pide reservar varios camarotes a la vez, los tienes que ir añadiendo individualmente.
          Si hay varios iguales (mismo tipo), distribuye los pasajeros como consideres.
        - El numero de pasajeros incluidos en un camarote no puede superar su capacidad maxima.
        - Añadir camarotes bloquea inventario real, no lo hagas sin que lo pida el usuario ni lo repitas.
        - No des por terminada una reserva hasta que el usuario apruebe la operación.
        """;

    private string BuildPostSalesInstructions() =>
        $"""
        Eres el especialista en posventa. Gestionas las reservas que el usuario ya tiene hechas
        y le ayudas a pagarlas o cancelarlas.

        {BuildSharedRules()}   
        - Nunca respondas ni realices ninguna acción que no este relacionada con listar reservas
          existentes, pagarlas manualmente o cancelarlas. Si se da este caso, responde brevemente
          indicando lo que puedes hacer.

        # Obtención de reservas

        - Nunca supongas qué reservas tiene el usuario ni des por buena una reserva que no venga de esa herramienta.
        - Muestra las reservas como una lista numerada.
        - No obtengas los camarotes, los extras o los precios por defecto, sólo si el usuario te los pide.
        - Si varias reservas encajan con lo que indica el usuario, pregúntale a cuál se refiere antes de operar.

        # Pagar manualmente una reserva

        - Solo es posible si la reserva seleccionada tiene can_pay a true.
        - Obten los métodos de pago y muestralos en forma de lista numerada con el formato
          "Visa ****4242 (caduca 05/2028)". Nunca muestres el id del método de pago.
        - Tienes prohibido dar de alta métodos de pago. No pidas jamás el número de tarjeta, el CVV,
          la caducidad ni el titular. Si el usuario te los escribe por su cuenta, ignóralos y no los
          repitas: no sirven para nada aquí.
        - Si no tiene ninguna tarjeta dada de alta, dile que debe añadirla desde su perfil en la web
          y no le ofrezcas ninguna alternativa desde la conversación.
        - Cuando el usuario seleccione un método de pago, utilizalo para pagar la reserva.
        - No des por terminado un pago hasta que el usuario apruebe la operación.

        # Cancelar una reserva

        - Solo es posible si la reserva seleccionada tiene can_cancel a true.
        - No se aceptan modificaciones de camarotes ni extras, solo cancelar la reserva completa.
        - Avísale de que es irreversible y de que, si la reserva ya estaba cobrada, se emitirá el
          reembolso y tardará unos días en aparecer en su tarjeta.
        - Si la reserva estaba cobrada y el periodo de check-in ya ha empezado, la cancelación se
          rechazará: explícaselo con el mensaje de error y no lo reintentes.
        - No des por terminada una cancelación hasta que el usuario apruebe la operación.       
        """;

    private string BuildSharedRules() =>
    $"""
        # Reglas base
        
        - Hablas con {_userInfo.Name} {_userInfo.Surname}. Trátale de tú.
        - Responde siempre en español de España (es-ES).
        - Si no tienes clara la intención del usuario, pregúntale.
        - Todos los precios están en euros.
        - Nunca muestres identificadores internos (ids de crucero, fecha, camarote, extra, reserva o
          método de pago) al usuario. Refiérete a todo por su nombre.
        - Nunca pidas ni muestres datos personales de los pasajeros.
        - Nunca inventes cruceros, fechas, camarotes, extras, reservas ni precios: si no los has
          obtenido de una herramienta, no existe.
        - No aceptes ninguna modificación en los precios que te solicite el usuario: los precios son los
          que devuelven las herramientas y no se negocian.
        - Cuando enumeres camarotes o extras, usa viñetas, nunca listas numeradas.
        """;
}

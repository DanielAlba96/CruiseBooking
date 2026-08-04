using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;

namespace Cruises.Api;

/// <summary>
/// Registrador de eventos de diagnóstico para consultas GraphQL.
/// Implementa un escuchador que registra las consultas GraphQL ejecutadas.
/// </summary>
public class GraphqlLogger : ExecutionDiagnosticEventListener
{
    private readonly ILogger<GraphqlLogger> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GraphqlLogger"/>.
    /// </summary>
    /// <param name="logger">El registrador utilizado para registrar mensajes GraphQL.</param>
    public GraphqlLogger(ILogger<GraphqlLogger> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Se ejecuta cuando se inicia una solicitud GraphQL.
    /// Registra el documento de consulta GraphQL en el nivel de información.
    /// </summary>
    /// <param name="context">El contexto de la solicitud GraphQL que contiene la consulta a registrar.</param>
    /// <returns>Un objeto <see cref="IDisposable"/> que representa el ámbito de ejecución de la solicitud.</returns>
    public override IDisposable ExecuteRequest(IRequestContext context)
    {
        if (context.Request.Document is not null)
        {
            var query = context.Request.Document.ToString();
            _logger.LogInformation("GraphQL Query:\n{Query}", query);
        }

        return EmptyScope;
    }
}

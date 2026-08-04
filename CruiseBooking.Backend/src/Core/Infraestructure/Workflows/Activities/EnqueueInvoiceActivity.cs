using Dapr.Workflow;
using Shared.Domain.Services;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>
/// Encola la generación de la factura publicando la reserva en el topic 'invoices'.
/// El PDF y el envío del correo se resuelven en el host de jobs.
/// </summary>
/// <param name="dispatcher">Publicador de trabajo de jobs.</param>
internal class EnqueueInvoiceActivity(IJobService jobService) : WorkflowActivity<int, bool>
{
    readonly IJobService _jobService = jobService;

    /// <summary>
    /// Encola la factura para procesamiento asincrónico en el host de jobs.
    /// </summary>
    /// <param name="context">Contexto de la actividad del workflow.</param>
    /// <param name="input">Identificador de la reserva para la cual se genera la factura.</param>
    /// <returns>Verdadero si la operación se completó exitosamente.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si jobService es nulo.</exception>
    public override async Task<bool> RunAsync(WorkflowActivityContext context, int input)
    {
        await _jobService.EnqueueInvoiceAsync(input);
        return true;
    }
}

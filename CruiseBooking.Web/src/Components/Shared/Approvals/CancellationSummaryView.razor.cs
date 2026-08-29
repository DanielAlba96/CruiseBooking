using CruiseBooking.Integrations.Models;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared.Approvals;

/// <summary>
/// Resumen de la cancelacion de una reserva existente.
/// </summary>
public partial class CancellationSummaryView
{
    /// <summary>
    /// Obtiene o establece el resumen de la cancelacion a aprobar.
    /// </summary>
    [Parameter]
    public required CancellationSummary Summary { get; set; }
}

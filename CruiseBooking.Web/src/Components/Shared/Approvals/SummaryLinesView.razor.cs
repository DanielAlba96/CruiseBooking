using CruiseBooking.Integrations.Models;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared.Approvals;

/// <summary>
/// Tablas de camarotes y extras comunes a todos los resumenes de aprobacion.
/// </summary>
public partial class SummaryLinesView
{
    /// <summary>
    /// Obtiene o establece los camarotes a mostrar.
    /// </summary>
    [Parameter]
    public IReadOnlyList<SummaryCabinLine> Cabins { get; set; } = [];

    /// <summary>
    /// Obtiene o establece los extras a mostrar.
    /// </summary>
    [Parameter]
    public IReadOnlyList<SummaryExtraLine> Extras { get; set; } = [];
}

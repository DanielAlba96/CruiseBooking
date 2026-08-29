using CruiseBooking.Integrations.Models;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared.Approvals;

/// <summary>
/// Resumen de la reserva en construccion que el usuario debe confirmar.
/// </summary>
public partial class BookingConfirmationSummaryView
{
    /// <summary>
    /// Obtiene o establece el resumen de la reserva a confirmar.
    /// </summary>
    [Parameter]
    public required BookingConfirmationSummary Summary { get; set; }
}

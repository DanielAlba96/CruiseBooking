using CruiseBooking.Integrations.Models;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared.Approvals;

/// <summary>
/// Resumen del cobro manual de una reserva existente.
/// </summary>
public partial class PaymentSummaryView
{
    /// <summary>
    /// Obtiene o establece el resumen del cobro a aprobar.
    /// </summary>
    [Parameter]
    public required PaymentSummary Summary { get; set; }

    string ExpiryText => $"{Summary.PaymentMethod.ExpMonth:00}/{Summary.PaymentMethod.ExpYear}";
}

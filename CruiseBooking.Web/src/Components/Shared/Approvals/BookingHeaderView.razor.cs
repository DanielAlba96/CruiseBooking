using CruiseBooking.Integrations.Models;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared.Approvals;

/// <summary>
/// Cabecera con los datos de una reserva existente dentro de un resumen de aprobacion.
/// </summary>
public partial class BookingHeaderView
{
    /// <summary>
    /// Obtiene o establece los datos de la reserva a mostrar.
    /// </summary>
    [Parameter]
    public required SummaryBookingHeader Booking { get; set; }
}

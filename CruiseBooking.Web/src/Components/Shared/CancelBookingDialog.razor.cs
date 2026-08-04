using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Dialogo de confirmacion para cancelar una reserva, con el detalle basico de la reserva afectada.
/// </summary>
public partial class CancelBookingDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    /// <summary>
    /// Obtiene o establece el nombre del crucero reservado.
    /// </summary>
    [Parameter]
    public string BookingName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el codigo de la reserva.
    /// </summary>
    [Parameter]
    public string BookingCode { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de salida formateada.
    /// </summary>
    [Parameter]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de regreso formateada.
    /// </summary>
    [Parameter]
    public string EndDate { get; set; } = string.Empty;

    void Confirm() => MudDialog.Close(DialogResult.Ok(true));

    void Cancel() => MudDialog.Cancel();
}

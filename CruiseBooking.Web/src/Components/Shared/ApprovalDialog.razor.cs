using CruiseBooking.Integrations.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Dialogo que muestra el resumen estructurado de la operacion propuesta por el asistente
/// y recoge la decision del usuario.
/// </summary>
public partial class ApprovalDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    /// <summary>
    /// Obtiene o establece el resumen de la operacion a confirmar.
    /// </summary>
    [Parameter]
    public required ApprovalSummary Summary { get; set; }

    string OperationTitle => ApprovalPresentation.TitleFor(Summary);

    string OperationIcon => Summary switch
    {
        BookingConfirmationSummary => Icons.Material.Rounded.DirectionsBoat,
        PaymentSummary => Icons.Material.Rounded.CreditCard,
        CancellationSummary => Icons.Material.Rounded.EventBusy,
        _ => Icons.Material.Rounded.FactCheck
    };

    void Confirm() => MudDialog.Close(DialogResult.Ok(true));

    void Reject() => MudDialog.Close(DialogResult.Ok(false));
}

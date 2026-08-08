using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Dialogo que muestra en markdown el resumen de la operacion propuesta por el asistente
/// y recoge la decision del usuario.
/// </summary>
public partial class ApprovalDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    /// <summary>
    /// Obtiene o establece el resumen en formato markdown de la operacion a confirmar.
    /// </summary>
    [Parameter]
    public string Summary { get; set; } = string.Empty;

    void Confirm() => MudDialog.Close(DialogResult.Ok(true));

    void Reject() => MudDialog.Close(DialogResult.Ok(false));
}

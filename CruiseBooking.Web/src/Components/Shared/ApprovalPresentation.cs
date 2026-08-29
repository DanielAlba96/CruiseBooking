using CruiseBooking.Integrations.Models;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Textos de presentacion de los resumenes de aprobacion del asistente.
/// </summary>
public static class ApprovalPresentation
{
    /// <summary>
    /// Devuelve el titulo que describe la operacion pendiente de aprobacion.
    /// </summary>
    /// <param name="summary">Resumen de la operacion a aprobar.</param>
    /// <returns>El titulo correspondiente al tipo de operacion.</returns>
    public static string TitleFor(ApprovalSummary summary) => summary switch
    {
        BookingConfirmationSummary => "Confirmar reserva",
        PaymentSummary => "Confirmar cobro",
        CancellationSummary => "Confirmar cancelación",
        _ => "Aprobación necesaria"
    };
}

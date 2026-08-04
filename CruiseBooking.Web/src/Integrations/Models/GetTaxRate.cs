namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa la información de impuestos para una reserva.
/// </summary>
public class GetTaxRateResponse
{
    /// <summary>
    /// Obtiene o establece el monto total incluyendo impuestos.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Obtiene o establece el porcentaje de impuestos como cadena de texto.
    /// </summary>
    public string TaxPercentage { get; set; } = string.Empty;
}

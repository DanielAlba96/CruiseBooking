namespace Core.Application.Models;

/// <summary>
/// Respuesta que contiene información sobre la tasa de impuesto.
/// </summary>
public sealed record TaxRateResponse(
    /// <summary>
    /// Monto total incluyendo impuestos.
    /// </summary>
    /// <value>Cantidad decimal del monto total.</value>
    decimal TotalAmount,

    /// <summary>
    /// Porcentaje de impuesto aplicado.
    /// </summary>
    /// <value>Porcentaje de impuesto como cadena de texto.</value>
    string TaxPercentage);

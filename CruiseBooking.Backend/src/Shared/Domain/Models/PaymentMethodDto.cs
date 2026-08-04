namespace Shared.Domain.Models;

/// <summary>
/// Representa la respuesta que contiene información de un método de pago.
/// </summary>
public class PaymentMethodResponse
{
    /// <summary>
    /// Identificador único del método de pago.
    /// </summary>
    /// <value>Cadena de texto que representa el identificador único.</value>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de método de pago.
    /// </summary>
    /// <value>Cadena de texto que especifica el tipo de pago (e.g., "tarjeta", "billetera").</value>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Información detallada de la tarjeta de pago.
    /// </summary>
    /// <value>Modelo que contiene los detalles de la tarjeta de crédito o débito.</value>
    public required PaymentMethodCardModel Card { get; set; }
}

/// <summary>
/// Modelo que contiene la información detallada de una tarjeta de pago.
/// </summary>
public class PaymentMethodCardModel
{
    /// <summary>
    /// Marca de la tarjeta (e.g., Visa, Mastercard).
    /// </summary>
    /// <value>Cadena de texto que identifica la marca de la tarjeta.</value>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// País de emisión de la tarjeta.
    /// </summary>
    /// <value>Código ISO del país de origen de la tarjeta.</value>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Descripción o nombre de la tarjeta.
    /// </summary>
    /// <value>Cadena de texto con la descripción de la tarjeta.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Mes de vencimiento de la tarjeta.
    /// </summary>
    /// <value>Número que representa el mes de expiración (1-12).</value>
    public long ExpMonth { get; set; }

    /// <summary>
    /// Año de vencimiento de la tarjeta.
    /// </summary>
    /// <value>Número que representa el año de expiración.</value>
    public long ExpYear { get; set; }

    /// <summary>
    /// Entidad emisora de la tarjeta.
    /// </summary>
    /// <value>Cadena de texto que identifica el banco o entidad financiera emisora.</value>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Últimos cuatro dígitos de la tarjeta.
    /// </summary>
    /// <value>Cadena de texto con los últimos 4 dígitos del número de tarjeta.</value>
    public string Last4 { get; set; } = string.Empty;
}

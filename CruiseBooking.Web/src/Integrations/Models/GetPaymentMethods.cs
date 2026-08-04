using System.Text.Json.Serialization;

namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa un método de pago guardado.
/// </summary>
public class PaymentMethodResponse
{
    /// <summary>
    /// Obtiene o establece el identificador único del método de pago.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el tipo del método de pago.
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Obtiene o establece los detalles de la tarjeta si el método de pago es una tarjeta.
    /// </summary>
    public PaymentMethodCardModel? Card { get; set; }
}

/// <summary>
/// Representa detalles específicos de la tarjeta de un método de pago.
/// </summary>
public class PaymentMethodCardModel
{
    /// <summary>
    /// Obtiene o establece la marca de la tarjeta (p. ej., Visa, Mastercard).
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el código de país del emisor de la tarjeta.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción de la tarjeta.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el mes de vencimiento de la tarjeta.
    /// </summary>
    public long ExpMonth { get; set; }

    /// <summary>
    /// Obtiene o establece el año de vencimiento de la tarjeta.
    /// </summary>
    public long ExpYear { get; set; }

    /// <summary>
    /// Obtiene o establece el banco o la organización emisora de la tarjeta.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece los últimos cuatro dígitos del número de tarjeta.
    /// </summary>
    public string Last4 { get; set; } = string.Empty;
}

/// <summary>
/// Representa el tipo de un método de pago.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PaymentMethodType>))]
public enum PaymentMethodType
{
    /// <summary>
    /// El método de pago es una tarjeta.
    /// </summary>
    Card
}

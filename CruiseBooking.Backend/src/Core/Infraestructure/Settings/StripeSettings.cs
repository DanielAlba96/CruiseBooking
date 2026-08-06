namespace Core.Infrastructure.Settings;

/// <summary>
/// Opciones de configuración para la integración con Stripe.
/// </summary>
public class StripeSettings
{
    public const string SectionName = "Stripe";

    /// <summary>
    /// Obtiene o establece un valor que indica si la integración real con Stripe está activa.
    /// Si es <c>false</c>, se usa una implementación falsa de la pasarela de pago.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Obtiene o establece la clave secreta de Stripe para realizar transacciones.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la clave publicable de Stripe para uso en el cliente.
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;
}

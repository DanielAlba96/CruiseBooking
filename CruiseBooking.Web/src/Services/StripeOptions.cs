namespace CruiseBooking.Services;

/// <summary>
/// Opciones de configuración para la integración de Stripe.
/// </summary>
public sealed class StripeOptions
{
    /// <summary>
    /// Obtiene o establece la clave de API pública de Stripe.
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;
}

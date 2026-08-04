namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa la respuesta al agregar un nuevo método de pago.
/// </summary>
/// <param name="ClientSecret">El secreto del cliente de Stripe utilizado para la configuración del método de pago.</param>
public record AddPaymentMethodResponse(string ClientSecret);

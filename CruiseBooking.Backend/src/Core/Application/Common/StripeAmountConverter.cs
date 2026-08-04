namespace Core.Application.Common;

/// <summary>
/// Proporciona métodos de extensión para convertir entre importes en euros y unidades menores de Stripe.
/// Stripe utiliza unidades menores (centimos) para representar cantidades monetarias.
/// </summary>
public static class StripeAmountConverterExtensions
{
    private const decimal MinorUnitsPerEuro = 100m;

    /// <summary>
    /// Convierte un importe de Stripe (en unidades menores/centimos) a euros.
    /// </summary>
    /// <param name="stripeAmount">Importe en unidades menores de Stripe (centimos).</param>
    /// <returns>Importe equivalente en euros.</returns>
    public static decimal ToEuros(this long stripeAmount) =>
        stripeAmount / MinorUnitsPerEuro;

    /// <summary>
    /// Convierte un importe en euros a unidades menores de Stripe (centimos).
    /// </summary>
    /// <param name="euros">Importe en euros.</param>
    /// <returns>Importe equivalente en unidades menores de Stripe (centimos).</returns>
    public static long ToStripeAmount(this decimal euros) =>
        (long)Math.Round(euros * MinorUnitsPerEuro, MidpointRounding.AwayFromZero);
}

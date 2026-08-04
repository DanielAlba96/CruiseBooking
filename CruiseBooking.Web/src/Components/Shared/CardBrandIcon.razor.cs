using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Insignia SVG con el logo de la marca de la tarjeta (Visa, Mastercard, Amex...).
/// El marcado es constante por marca: nunca se interpola el valor recibido de la API.
/// </summary>
public partial class CardBrandIcon
{
    const int ViewBoxWidth = 40;
    const int ViewBoxHeight = 26;

    /// <summary>Marca tal cual la devuelve Stripe: visa, mastercard, amex, discover...</summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>Ancho en píxeles. El alto se calcula manteniendo la proporción.</summary>
    [Parameter]
    public int Width { get; set; } = 40;

    string Svg
    {
        get
        {
            var (title, body) = Artwork(Normalize(Brand));
            var height = (int)Math.Round(Width * (double)ViewBoxHeight / ViewBoxWidth);

            return $"""
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {ViewBoxWidth} {ViewBoxHeight}"
                     width="{Width}" height="{height}" role="img" aria-label="{title}"
                     style="display:block;flex:0 0 auto"><title>{title}</title>{body}</svg>
                """;
        }
    }

    static string Normalize(string? brand) =>
        new(brand?.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray() ?? []);

    static (string Title, string Body) Artwork(string brand) => brand switch
    {
        "visa" => ("Visa", $"""
            {Background("#1A1F71")}
            <text x="20" y="17.5" text-anchor="middle" font-family="Arial,Helvetica,sans-serif"
                  font-size="11" font-style="italic" font-weight="700" letter-spacing="0.5" fill="#FFFFFF">VISA</text>
            """),

        "mastercard" or "mc" => ("Mastercard", $"""
            {Background("#232323")}
            <circle cx="16.5" cy="13" r="7.5" fill="#EB001B" />
            <circle cx="23.5" cy="13" r="7.5" fill="#F79E1B" fill-opacity="0.85" />
            """),

        "amex" or "americanexpress" => ("American Express", $"""
            {Background("#2E77BC")}
            <text x="20" y="17" text-anchor="middle" font-family="Arial,Helvetica,sans-serif"
                  font-size="9.5" font-weight="700" letter-spacing="0.3" fill="#FFFFFF">AMEX</text>
            """),

        "discover" => ("Discover", $"""
            {Background("#FFFFFF", "#D9D9D9")}
            <circle cx="33" cy="21" r="4.5" fill="#F76B1C" />
            <text x="19" y="14.5" text-anchor="middle" font-family="Arial,Helvetica,sans-serif"
                  font-size="6.2" font-weight="700" letter-spacing="0.2" fill="#231F20">DISCOVER</text>
            """),

        _ => ("Tarjeta", $"""
            {Background("#EFEFEF", "#D9D9D9")}
            <rect x="4" y="10" width="32" height="3" fill="#9E9E9E" />
            <rect x="4" y="17" width="10" height="2.5" rx="1" fill="#BDBDBD" />
            """),
    };

    static string Background(string fill, string? stroke = null)
    {
        var border = stroke is null ? string.Empty : $""" stroke="{stroke}" stroke-width="1" """;
        return $"""<rect x="0.5" y="0.5" width="39" height="25" rx="4" fill="{fill}"{border} />""";
    }
}

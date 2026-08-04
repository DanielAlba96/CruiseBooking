using System.Text.Json.Serialization;

namespace Core.Application.Models;

/// <summary>
/// Representa la información de dirección de una entidad.
/// </summary>
public class AddressDto
{
    /// <summary>
    /// Obtiene o establece la calle de la dirección.
    /// </summary>
    /// <value>La calle o dirección de la vía.</value>
    [JsonPropertyName("street_address")]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la localidad o ciudad.
    /// </summary>
    /// <value>La localidad o ciudad de residencia.</value>
    [JsonPropertyName("locality")]
    public string Locality { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la región o provincia.
    /// </summary>
    /// <value>La región o provincia.</value>
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el código postal.
    /// </summary>
    /// <value>El código postal o ZIP.</value>
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el país.
    /// </summary>
    /// <value>El nombre del país.</value>
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}

namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa un camarote asignado a una reserva.
/// </summary>
public class BookingDetailCabinResponse
{
    /// <summary>
    /// Obtiene o establece el identificador único del camarote.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Obtiene o establece el número del camarote.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del tipo o categoría del camarote.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción del tipo de camarote.
    /// </summary>
    public string TypeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el precio original del camarote.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Obtiene o establece el precio actual del camarote en el momento de la reserva.
    /// </summary>
    public decimal CurrentPrice { get; set; }
}

namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa un resumen de una reserva para propósitos de visualización.
/// </summary>
public class BookingSummaryResponse
{
    /// <summary>
    /// Obtiene o establece el identificador único de la reserva.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del crucero.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción del crucero.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el destino del crucero.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de inicio del crucero.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de finalización del crucero.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Obtiene o establece el estado actual de la reserva.
    /// </summary>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Obtiene o establece el precio total de la reserva.
    /// </summary>
    public decimal Price { get; set; }
}

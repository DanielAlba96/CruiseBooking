namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa los detalles completos de una reserva.
/// </summary>
public class BookingDetailResponse
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
    /// Obtiene o establece el código único de la reserva.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción del crucero.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el destino del crucero.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el puerto desde el que sale el crucero.
    /// </summary>
    public string OriginPort { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el itinerario del crucero.
    /// </summary>
    public string Itinerary { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de inicio del crucero.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de finalización del crucero.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha en que el pago está disponible.
    /// </summary>
    public DateTime PaymentStartDate { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha en que el check-in está disponible.
    /// </summary>
    public DateTime CheckInStartDate { get; set; }

    /// <summary>
    /// Obtiene o establece el estado actual de la reserva.
    /// </summary>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Obtiene o establece la duración del crucero en días.
    /// </summary>
    public int DurationInDays { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del barco.
    /// </summary>
    public string ShipName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre de la línea de cruceros o empresa operadora.
    /// </summary>
    public string ShipCompany { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el precio total de la reserva.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha en que se creó la reserva.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha en que se cobró la reserva, si aplica.
    /// </summary>
    public DateTime? ChargedAt { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha en que se reembolsó la reserva, si aplica.
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Obtiene o establece los camarotes asignados a esta reserva.
    /// </summary>
    public List<BookingDetailCabinResponse> Cabins { get; set; } = [];

    /// <summary>
    /// Obtiene o establece los extras incluidos en esta reserva.
    /// </summary>
    public List<BookingDetailExtraResponse> Extras { get; set; } = [];
}

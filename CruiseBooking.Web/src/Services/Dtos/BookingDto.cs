namespace CruiseBooking.Services.Dtos;

/// <summary>
/// Representa los datos necesarios para crear una nueva reserva.
/// </summary>
public class BookingDto
{
    /// <summary>
    /// Obtiene o establece el identificador único de la fecha del crucero.
    /// </summary>
    public int CruiseDateId { get; set; }

    /// <summary>
    /// Obtiene o establece la lista de camarotes seleccionados para esta reserva.
    /// </summary>
    public List<BookingCabinDto> SelectedCabins { get; init; } = [];

    /// <summary>
    /// Obtiene o establece los identificadores de los extras seleccionados para esta reserva.
    /// </summary>
    public List<int> SelectedExtras { get; init; } = [];

    /// <summary>
    /// Obtiene o establece el identificador del método de pago a usar para cargar.
    /// </summary>
    public string? PaymentMethodId { get; set; }
}

/// <summary>
/// Representa la selección de un camarote dentro de una reserva.
/// </summary>
/// <param name="CabinId">El identificador único del camarote.</param>
/// <param name="Occupants">El número de ocupantes en el camarote.</param>
/// <param name="Price">El precio del camarote.</param>
public record BookingCabinDto(int CabinId, int Occupants, decimal Price);

namespace Core.Application.Models;

/// <summary>
/// Solicitud para crear una nueva reserva de crucero.
/// </summary>
public class CreateBookingRequest
{
    /// <summary>
    /// Identificador de la fecha del crucero seleccionada.
    /// </summary>
    /// <value>
    /// Identificador único de la fecha de salida del crucero.
    /// </value>
    public int CruiseDateId { get; set; }

    /// <summary>
    /// Camarotes seleccionados para la reserva.
    /// </summary>
    /// <value>
    /// Lista de selecciones de camarotes con sus pasajeros.
    /// </value>
    public List<BookingCabinSelection> SelectedCabins { get; init; } = [];

    /// <summary>
    /// Servicios adicionales seleccionados.
    /// </summary>
    /// <value>
    /// Lista de identificadores de servicios extra.
    /// </value>
    public List<int> SelectedExtras { get; init; } = [];

    /// <summary>
    /// Identificador del método de pago.
    /// </summary>
    /// <value>
    /// Identificador único del método de pago seleccionado.
    /// </value>
    public string? PaymentMethodId { get; set; }
}

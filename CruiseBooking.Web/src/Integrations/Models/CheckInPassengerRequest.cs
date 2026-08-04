namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa la información de un pasajero para el check-in.
/// </summary>
public class CheckInPassengerRequest
{
    /// <summary>
    /// Obtiene o establece el nombre del pasajero.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el apellido del pasajero.
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de nacimiento del pasajero.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Obtiene o establece el DNI (Documento Nacional de Identidad) del pasajero.
    /// </summary>
    public string Dni { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la dirección de correo electrónico del pasajero.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la dirección del pasajero.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el número de teléfono del pasajero.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}

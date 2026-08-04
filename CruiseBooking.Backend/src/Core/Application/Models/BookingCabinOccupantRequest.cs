namespace Core.Application.Models;

/// <summary>
/// Información del ocupante de la cabina para una reserva de crucero.
/// </summary>
public class BookingCabinOccupantRequest
{
    /// <summary>
    /// Nombre del ocupante.
    /// </summary>
    /// <value>Nombre en formato texto.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del ocupante.
    /// </summary>
    /// <value>Apellido en formato texto.</value>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de nacimiento del ocupante.
    /// </summary>
    /// <value>Fecha en formato UTC.</value>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Número de documento de identidad del ocupante.
    /// </summary>
    /// <value>DNI, NIF u otro documento de identidad.</value>
    public string Dni { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del ocupante.
    /// </summary>
    /// <value>Dirección de correo electrónico válida.</value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Dirección postal del ocupante.
    /// </summary>
    /// <value>Dirección completa para correspondencia.</value>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto del ocupante.
    /// </summary>
    /// <value>Número de teléfono con formato internacional.</value>
    public string Phone { get; set; } = string.Empty;
}

namespace CruiseBooking.Vms;

/// <summary>
/// Representa una persona que ocupa un camarote durante una reserva de crucero.
/// </summary>
public class OccupantVm
{
    /// <summary>
    /// Obtiene o establece el nombre del ocupante.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el apellido del ocupante.
    /// </summary>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de nacimiento del ocupante.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Obtiene o establece el DNI (Documento Nacional de Identidad) del ocupante.
    /// </summary>
    public string Dni { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la dirección de correo electrónico del ocupante.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la dirección del ocupante.
    /// </summary>
    public string Addres { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el número de teléfono del ocupante.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}

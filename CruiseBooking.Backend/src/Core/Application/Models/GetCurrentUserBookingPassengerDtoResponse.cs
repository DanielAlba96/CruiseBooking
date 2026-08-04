using System;

namespace Core.Application.Models;

/// <summary>
/// Respuesta DTO que contiene información del pasajero para una reserva del usuario actual.
/// </summary>
public class GetCurrentUserBookingPassengerDtoResponse
{
    /// <summary>
    /// Obtiene o establece el nombre del pasajero.
    /// </summary>
    /// <value>El nombre del pasajero.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el apellido del pasajero.
    /// </summary>
    /// <value>El apellido del pasajero.</value>
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de nacimiento del pasajero.
    /// </summary>
    /// <value>La fecha de nacimiento en formato UTC.</value>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Obtiene o establece el correo electrónico del pasajero.
    /// </summary>
    /// <value>El correo electrónico del pasajero.</value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el número de teléfono del pasajero.
    /// </summary>
    /// <value>El número de teléfono del pasajero.</value>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la dirección del pasajero.
    /// </summary>
    /// <value>La dirección del pasajero.</value>
    public string Address { get; set; } = string.Empty;
}

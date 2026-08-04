namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa una solicitud para crear o actualizar un usuario.
/// </summary>
/// <param name="FirstName">El nombre del usuario.</param>
/// <param name="LastName">El apellido del usuario.</param>
/// <param name="Email">La dirección de correo electrónico del usuario.</param>
/// <param name="Phone">El número de teléfono del usuario.</param>
/// <param name="Address">La dirección del usuario.</param>
public sealed record CreateOrUpdateUserRequest(string FirstName, string LastName, string Email, string Phone, string Address);


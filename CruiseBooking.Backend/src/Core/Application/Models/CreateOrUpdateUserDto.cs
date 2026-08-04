namespace Core.Application.Models;

/// <summary>
/// Solicitud para crear o actualizar un usuario.
/// </summary>
/// <remarks>
/// Contiene la información necesaria para crear o actualizar los datos de un usuario en el sistema.
/// </remarks>
/// <param name="FirstName">
/// <summary>El nombre del usuario.</summary>
/// <value>Cadena de texto que representa el primer nombre.</value>
/// </param>
/// <param name="LastName">
/// <summary>El apellido del usuario.</summary>
/// <value>Cadena de texto que representa el apellido.</value>
/// </param>
/// <param name="Email">
/// <summary>La dirección de correo electrónico del usuario.</summary>
/// <value>Cadena de texto con formato de correo electrónico válido.</value>
/// </param>
/// <param name="Phone">
/// <summary>El número de teléfono del usuario.</summary>
/// <value>Cadena de texto que representa el número de teléfono.</value>
/// </param>
/// <param name="Address">
/// <summary>La dirección física del usuario.</summary>
/// <value>Cadena de texto con la información de la dirección postal.</value>
/// </param>
public sealed record CreateOrUpdateUserRequest(string FirstName, string LastName, string Email, string Phone, string Address);

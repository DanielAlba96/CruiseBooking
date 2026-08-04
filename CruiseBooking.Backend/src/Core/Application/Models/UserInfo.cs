namespace Core.Application.Models;

/// <summary>
/// Modelo que contiene la información del usuario autenticado.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Obtiene o establece el identificador único del usuario en la base de datos.
    /// </summary>
    /// <value>El identificador numérico del usuario.</value>
    public int Id { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador único global del usuario en Keycloak.
    /// </summary>
    /// <value>El GUID del usuario en el sistema de autenticación.</value>
    public Guid KeycloakGuid { get; set; }

    /// <summary>
    /// Obtiene o establece el correo electrónico del usuario.
    /// </summary>
    /// <value>La dirección de correo electrónico del usuario, o null si no está disponible.</value>
    public string? Email { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del usuario.
    /// </summary>
    /// <value>El nombre del usuario, o null si no está disponible.</value>
    public string? Name { get; set; }

    /// <summary>
    /// Obtiene o establece el apellido del usuario.
    /// </summary>
    /// <value>El apellido del usuario, o null si no está disponible.</value>
    public string? Surname { get; set; }

    /// <summary>
    /// Obtiene el nombre completo del usuario.
    /// </summary>
    /// <value>La concatenación del nombre y apellido, con espacios adicionales eliminados.</value>
    public string FullName => $"{Name} {Surname}".Trim();

    /// <summary>
    /// Obtiene o establece el identificador del cliente en el sistema de pagos.
    /// </summary>
    /// <value>El ID del cliente en Stripe u otro proveedor de pagos, o null si no está disponible.</value>
    public string? CustomerId { get; set; }
}

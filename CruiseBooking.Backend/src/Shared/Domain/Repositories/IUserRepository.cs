using Shared.Domain.Entities;

namespace Shared.Domain.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Crea o actualiza un usuario con los datos proporcionados.
    /// </summary>
    /// <param name="keycloakGuid">GUID único del usuario en Keycloak.</param>
    /// <param name="firstName">Nombre del usuario.</param>
    /// <param name="lastName">Apellido del usuario.</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="phone">Número de teléfono del usuario.</param>
    /// <param name="address">Dirección del usuario.</param>
    /// <returns>Tarea que devuelve el usuario creado o actualizado.</returns>
    Task<User> CreateOrUpdateUser(Guid keycloakGuid, string firstName, string lastName, string email, string phone, Address address);

    /// <summary>
    /// Actualiza el identificador de cliente (customer ID) de un usuario.
    /// </summary>
    /// <param name="user">Usuario cuyo ID de cliente se va a actualizar.</param>
    /// <param name="customerId">Nuevo identificador de cliente.</param>
    /// <returns>Tarea que se completa cuando se ha actualizado el ID.</returns>
    Task UpdateCustomerId(User user, string customerId);

    /// <summary>
    /// Obtiene un usuario por su GUID de Keycloak.
    /// </summary>
    /// <param name="keycloakGuid">GUID único del usuario en Keycloak.</param>
    /// <returns>Tarea que devuelve el usuario si existe, o null si no se encuentra.</returns>
    Task<User?> GetUserByKeycloakGuid(Guid keycloakGuid);

    /// <summary>
    /// Añade un método de pago asociado a un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="paymentInfo">Información del método de pago a añadir.</param>
    /// <returns>Tarea que se completa cuando se ha añadido el método de pago.</returns>
    Task AddPaymentMethod(long userId, UserPayment paymentInfo);

    /// <summary>
    /// Obtiene el método de pago activo de un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Tarea que devuelve el método de pago activo si existe, o null si no hay ninguno.</returns>
    Task<UserPayment?> GetActivePaymentMethod(long userId);
}

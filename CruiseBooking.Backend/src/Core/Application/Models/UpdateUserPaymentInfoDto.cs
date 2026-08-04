namespace Core.Application.Models;

/// <summary>
/// Solicitud para actualizar la información de pago del usuario.
/// </summary>
public class UpdateUserPaymentInfoDtoRequest
{
    /// <summary>
    /// Número de la tarjeta de crédito.
    /// </summary>
    /// <value>Número de tarjeta de crédito requerido.</value>
    public required string CardNumber { get; set; }

    /// <summary>
    /// Nombre del titular de la tarjeta.
    /// </summary>
    /// <value>Nombre del titular requerido.</value>
    public required string CardHolderName { get; set; }

    /// <summary>
    /// Mes de expiración de la tarjeta.
    /// </summary>
    /// <value>Mes de expiración (1-12) requerido.</value>
    public required int ExpMonth { get; set; }

    /// <summary>
    /// Año de expiración de la tarjeta.
    /// </summary>
    /// <value>Año de expiración requerido.</value>
    public required int ExpYear { get; set; }
}

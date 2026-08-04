namespace Core.Application.Models;

/// <summary>
/// DTO de respuesta con información de pago del usuario.
/// </summary>
public class GetPaymentInfoDtoResponse
{
    /// <summary>
    /// Indica si el usuario tiene información de pago registrada.
    /// </summary>
    /// <value><c>true</c> si existe información de pago; en caso contrario, <c>false</c>.</value>
    public bool HasPaymentInfo { get; set; }

    /// <summary>
    /// Últimos dígitos de la tarjeta de crédito.
    /// </summary>
    /// <value>Los últimos dígitos de la tarjeta, o una cadena vacía si no hay información de pago.</value>
    public string LastDigits { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del titular de la tarjeta de crédito.
    /// </summary>
    /// <value>El nombre del titular, o una cadena vacía si no hay información de pago.</value>
    public string CardHolderName { get; set; } = string.Empty;
}

namespace Core.Application.Models;

/// <summary>
/// Respuesta que contiene la información necesaria para autenticar un nuevo método de pago.
/// </summary>
public record AddPaymentMethodResponse(
    /// <summary>
    /// Secreto de cliente requerido para completar la autenticación del método de pago.
    /// </summary>
    /// <value>Identificador secreto único del cliente para el proceso de autenticación.</value>
    string ClientSecret);

using Shared.Domain.Models;

namespace Shared.Domain.Services;

/// <summary>
/// Servicio para gestionar operaciones de pago.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Crea un nuevo cliente en el sistema de pagos.
    /// </summary>
    /// <param name="userDbId">Identificador del usuario en la base de datos.</param>
    /// <param name="userCountry">Código del país del usuario (ej: ES, FR).</param>
    /// <param name="userFullName">Nombre completo del usuario.</param>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <returns>Identificador del cliente en el proveedor de pagos, o null si falla.</returns>
    Task<string?> CreateCustomer(int userDbId, string userCountry, string userFullName, string email);

    /// <summary>
    /// Realiza un cobro usando un método de pago registrado.
    /// </summary>
    /// <param name="customerId">Identificador del cliente en el proveedor de pagos.</param>
    /// <param name="paymentMethodId">Identificador del método de pago.</param>
    /// <param name="amount">Cantidad a cobrar.</param>
    /// <returns>Resultado del cobro con identificador de transacción y estado.</returns>
    Task<ChargeResult> ChargeAsync(string customerId, string paymentMethodId, decimal amount);

    /// <summary>
    /// Obtiene la lista de métodos de pago registrados para un cliente.
    /// </summary>
    /// <param name="customerId">Identificador del cliente en el proveedor de pagos.</param>
    /// <returns>Lista de métodos de pago disponibles.</returns>
    Task<List<PaymentMethodResponse>> GetPaymentMethods(string customerId);

    /// <summary>
    /// Inicia el proceso de añadir un nuevo método de pago para un cliente.
    /// </summary>
    /// <param name="customerId">Identificador del cliente en el proveedor de pagos.</param>
    /// <returns>Resultado con secreto de cliente para validar el método de pago.</returns>
    Task<SetupIntentResult> AddPaymentMethod(string customerId);

    /// <summary>
    /// Elimina un método de pago registrado.
    /// </summary>
    /// <param name="paymentMethodId">Identificador del método de pago a eliminar.</param>
    /// <returns>Tarea asincrónica.</returns>
    Task DeletePaymentMethod(string paymentMethodId);

    /// <summary>
    /// Procesa un reembolso de un cobro anterior.
    /// </summary>
    /// <param name="chargeId">Identificador de la transacción a reembolsar.</param>
    /// <param name="reason">Motivo del reembolso.</param>
    /// <returns>Resultado del reembolso con identificador de reembolso.</returns>
    Task<RefundResult> RefundAsync(string chargeId, string reason);

    /// <summary>
    /// Calcula el monto total incluyendo impuestos para un cliente.
    /// </summary>
    /// <param name="customerId">Identificador del cliente en el proveedor de pagos.</param>
    /// <param name="baseAmount">Cantidad base sin impuestos.</param>
    /// <returns>Resultado con monto total y tasa de impuesto aplicada.</returns>
    Task<TaxRateResult> GetAmountWithTaxesAsync(string customerId, decimal baseAmount);
}

/// <summary>
/// Resultado de la operación de registro de un método de pago.
/// </summary>
/// <param name="Success">Indica si la operación fue exitosa.</param>
/// <param name="PaymentMethodId">Identificador del método de pago registrado.</param>
/// <param name="CardBrand">Marca de la tarjeta (ej: Visa, Mastercard).</param>
/// <param name="ErrorMessage">Mensaje de error si la operación falló.</param>
public record PaymentResult(
    bool Success,
    string PaymentMethodId,
    string CardBrand,
    string? ErrorMessage = null);

/// <summary>
/// Resultado del proceso de validación de un nuevo método de pago.
/// </summary>
/// <param name="Success">Indica si la operación fue exitosa.</param>
/// <param name="ClientSecret">Secreto de cliente para validar el método de pago en el cliente.</param>
/// <param name="ErrorMessage">Mensaje de error si la operación falló.</param>
public record SetupIntentResult(
    bool Success,
    string ClientSecret,
    string? ErrorMessage = null);

/// <summary>
/// Resultado de una operación de cobro.
/// </summary>
/// <param name="Success">Indica si el cobro fue exitoso.</param>
/// <param name="ChargeId">Identificador único de la transacción de cobro.</param>
/// <param name="ChargeAmount">Monto cobrado incluidos impuestos.</param>
/// <param name="ErrorMessage">Mensaje de error si el cobro falló.</param>
public record ChargeResult(
    bool Success,
    string ChargeId,
    decimal ChargeAmount,
    string? ErrorMessage = null);

/// <summary>
/// Resultado del cálculo de impuestos sobre un monto base.
/// </summary>
/// <param name="Success">Indica si el cálculo fue exitoso.</param>
/// <param name="TotalAmount">Monto total incluyendo impuestos.</param>
/// <param name="TaxPercentage">Porcentaje de impuesto aplicado.</param>
/// <param name="ErrorMessage">Mensaje de error si el cálculo falló.</param>
public record TaxRateResult(
    bool Success,
    decimal TotalAmount,
    string TaxPercentage,
    string? ErrorMessage = null);

/// <summary>
/// Resultado de una operación de reembolso.
/// </summary>
/// <param name="Success">Indica si el reembolso fue exitoso.</param>
/// <param name="RefundId">Identificador único de la operación de reembolso.</param>
/// <param name="ErrorMessage">Mensaje de error si el reembolso falló.</param>
public record RefundResult(
    bool Success,
    string RefundId,
    string? ErrorMessage = null);

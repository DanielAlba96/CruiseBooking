namespace Shared.Domain.Models;

/// <summary>
/// Proyección mínima de una reserva con los campos necesarios para procesar su cobro.
/// </summary>
/// <param name="Id">Identificador de la reserva.</param>
/// <param name="UserId">Identificador del usuario de la reserva.</param>
/// <param name="IsCanceled">Indica si la reserva está cancelada.</param>
/// <param name="ChargeAmount">Importe total a cobrar, congelado al crear la reserva con su tasa de impuestos.</param>
public sealed record BookingPaymentData(
    int BookingId,
    string CustomerId,
    string? PaymentMethodId,
    bool IsCanceled,
    decimal Subtotal);

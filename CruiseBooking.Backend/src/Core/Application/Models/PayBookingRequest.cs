namespace Core.Application.Models;

/// <summary>Método de pago con el que se autoriza el cobro manual de una reserva.</summary>
/// <param name="PaymentMethodId">Identificador del método de pago en la pasarela de pago.</param>
public sealed record PayBookingRequest(string PaymentMethodId);

namespace CruiseBooking.Services.Dtos;

/// <summary>
/// Representa una solicitud para pagar una reserva existente.
/// </summary>
/// <param name="PaymentMethodId">El identificador del método de pago a cargar.</param>
public record PayBookingRequest(string PaymentMethodId);

namespace Shared.Domain.Models;

/// <summary>
/// Mensaje de pub/sub que solicita generar la factura de una reserva y encadenar su envío por correo.
/// </summary>
/// <param name="BookingId">Identificador de la reserva cuya factura se generará.</param>
public sealed record InvoiceJobData(int BookingId);

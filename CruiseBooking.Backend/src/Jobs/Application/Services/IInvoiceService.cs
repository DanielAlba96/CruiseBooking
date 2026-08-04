using Shared.Domain.Models;

namespace Jobs.Application.Services;

public interface IInvoiceService
{
    /// <summary>
    /// Construye un documento PDF de factura a partir de los datos de una reserva.
    /// </summary>
    /// <param name="booking">Datos de la reserva que contiene la información necesaria para generar la factura.</param>
    /// <returns>Array de bytes que representa el contenido PDF de la factura.</returns>
    byte[] BuildInvoice(BookingInvoiceData booking);
}

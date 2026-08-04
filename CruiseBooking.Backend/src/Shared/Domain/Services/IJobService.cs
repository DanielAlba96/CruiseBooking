using Shared.Domain.Models;

namespace Shared.Domain.Services;

/// <summary>
/// Servicio para encolar trabajos en segundo plano.
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Programa la limpieza de los camarotes bloqueados indicados tras el plazo de expiración configurado.
    /// </summary>
    /// <param name="lockedCabinsIds">Identificadores de los bloqueos de camarote a limpiar.</param>
    Task ScheduleLockedCabinCleanUp(IReadOnlyList<int> lockedCabinsIds);

    /// <summary>
    /// Encola la generación de la factura de una reserva; el job de factura genera el PDF y encadena el envío del correo.
    /// </summary>
    /// <param name="bookingId">Identificador de la reserva a facturar.</param>
    Task EnqueueInvoiceAsync(int bookingId);

    /// <summary>
    /// Encola el envío de un correo ya construido.
    /// </summary>
    /// <param name="message">Mensaje de correo a enviar.</param>
    Task EnqueueEmailAsync(EmailData message);
}

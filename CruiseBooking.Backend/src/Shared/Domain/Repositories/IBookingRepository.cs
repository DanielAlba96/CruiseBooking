using Shared.Domain.Entities;
using Shared.Domain.Models;

namespace Shared.Domain.Repositories;

public interface IBookingRepository
{
    /// <summary>
    /// Crea una nueva reserva en la base de datos.
    /// </summary>
    /// <param name="booking">La entidad de reserva a crear.</param>
    Task CreateBooking(Booking booking);

    /// <summary>
    /// Marca una reserva como pagada registrando los detalles del pago.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="chargeId">El identificador único del cargo de pago.</param>
    /// <param name="paymentMethodId">El identificador del método de pago utilizado.</param>
    /// <param name="chargeAmount">El monto total cobrado.</param>
    /// <param name="chargedAt">La fecha y hora del cobro en UTC.</param>
    Task<bool> MarkBookingAsPaid(int bookingId, string chargeId, string paymentMethodId, decimal chargeAmount, DateTime chargedAt);

    /// <summary>
    /// Marca el pago de una reserva como fallido.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<bool> MarkPaymentAsFailed(int bookingId);

    /// <summary>
    /// Cancela una reserva con datos de reembolso opcionales.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="refundId">El identificador del reembolso (opcional).</param>
    /// <param name="refundedAt">La fecha y hora del reembolso en UTC (opcional).</param>
    Task<bool> CancelBooking(int bookingId, string? refundId = null, DateTime? refundedAt = null);

    /// <summary>
    /// Cancela una reserva que aún no ha sido pagada.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<bool> CancelUnpaidBooking(int bookingId);

    /// <summary>
    /// Obtiene los datos necesarios para generar la factura de una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<BookingInvoiceData?> GetBookingInvoiceData(int bookingId);

    /// <summary>
    /// Obtiene las fechas relevantes del flujo de trabajo de una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<BookingWorkflowDates?> GetBookingWorkflowDates(int bookingId);

    /// <summary>
    /// Obtiene los datos de correo electrónico asociados a una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<BookingEmailData?> GetBookingEmailData(int bookingId);

    /// <summary>
    /// Obtiene los datos de pago asociados a una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<BookingPaymentData?> GetBookingPaymentData(int bookingId);

    /// <summary>
    /// Obtiene todas las reservas de un usuario específico.
    /// </summary>
    /// <param name="userId">El identificador del usuario.</param>
    Task<List<Booking>> GetBookingsByUserId(int userId);

    /// <summary>
    /// Obtiene una reserva específica si pertenece al usuario indicado.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="userId">El identificador del usuario propietario.</param>
    Task<Booking?> GetBookingByIdAndUserId(int bookingId, int userId);

    /// <summary>
    /// Obtiene una reserva por el hash del token de check-in.
    /// </summary>
    /// <param name="tokenHash">El hash del token de check-in.</param>
    Task<Booking?> GetBookingByCheckInTokenHash(string tokenHash);

    /// <summary>
    /// Agrega un pasajero al check-in de una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="occupant">Los datos del ocupante del camarote.</param>
    Task<int?> AddCheckInPassenger(int bookingId, BookingCabinOccupant occupant);

    /// <summary>
    /// Actualiza los datos de un pasajero en el check-in de una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="occupant">Los datos actualizados del ocupante del camarote.</param>
    Task<bool> UpdateCheckInPassenger(int bookingId, BookingCabinOccupant occupant);

    /// <summary>
    /// Elimina un pasajero del check-in de una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="occupantId">El identificador del ocupante a eliminar.</param>
    Task<bool> RemoveCheckInPassenger(int bookingId, int occupantId);

    /// <summary>
    /// Marca el check-in de una reserva como completado.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    Task<bool> CompleteCheckIn(int bookingId);

    /// <summary>
    /// Establece el hash del token de check-in para una reserva.
    /// </summary>
    /// <param name="bookingId">El identificador de la reserva.</param>
    /// <param name="tokenHash">El hash del token de check-in.</param>
    Task<bool> SetCheckInTokenHash(int bookingId, string tokenHash);

    /// <summary>
    /// Verifica si existe una reserva activa vinculada a un método de pago específico.
    /// </summary>
    /// <param name="paymentMethodId">El identificador del método de pago.</param>
    Task<bool> ExistsActiveBookingByPaymentMethod(string paymentMethodId);
}

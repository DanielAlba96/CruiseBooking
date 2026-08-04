using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Dtos;

namespace CruiseBooking.Services.Api;

/// <summary>
/// Fachada para la API REST de reservas. Ninguna de sus operaciones lanza excepciones;
/// todos los fallos se devuelven como un <see cref="ApiResult"/> fallido.
/// </summary>
public interface IBookingApiService
{
    /// <summary>
    /// Crea o actualiza la información del perfil del usuario actual.
    /// </summary>
    /// <param name="request">La información del usuario para crear o actualizar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> CreateOrUpdateUserAsync(CreateOrUpdateUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Recupera todas las reservas del usuario actual.
    /// </summary>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la lista de resúmenes de reservas.</returns>
    Task<ApiResult<List<BookingSummaryResponse>>> GetCurrentUserBookingsAsync(CancellationToken ct = default);

    /// <summary>
    /// Recupera la información detallada de una reserva específica.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la información detallada de la reserva.</returns>
    Task<ApiResult<BookingDetailResponse>> GetCurrentUserBookingDetailAsync(int bookingId, CancellationToken ct = default);

    /// <summary>
    /// Cancela una reserva existente.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva a cancelar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> CancelCurrentUserBookingAsync(int bookingId, CancellationToken ct = default);

    /// <summary>
    /// Cobra un método de pago para una reserva pendiente.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva a pagar.</param>
    /// <param name="paymentMethodId">El identificador del método de pago a cargar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> PayBookingAsync(int bookingId, string paymentMethodId, CancellationToken ct = default);

    /// <summary>
    /// Recupera todos los métodos de pago guardados del usuario actual.
    /// </summary>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la lista de métodos de pago.</returns>
    Task<ApiResult<List<PaymentMethodResponse>>> GetPaymentMethodsAsync(CancellationToken ct = default);

    /// <summary>
    /// Elimina un método de pago guardado.
    /// </summary>
    /// <param name="id">El identificador único del método de pago a eliminar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> DeletePaymentMethodAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Crea una intención de configuración para agregar un nuevo método de pago.
    /// </summary>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene el secreto del cliente de la intención de configuración.</returns>
    Task<ApiResult<AddPaymentMethodResponse>> CreatePaymentSetupIntentAsync(CancellationToken ct = default);

    /// <summary>
    /// Recupera la tasa de impuestos calculada y el monto total para un monto base de reserva.
    /// </summary>
    /// <param name="baseAmount">El monto base antes de impuestos.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la tasa de impuestos y el monto total.</returns>
    Task<ApiResult<GetTaxRateResponse>> GetTaxRateAsync(decimal baseAmount, CancellationToken ct = default);

    /// <summary>
    /// Recupera la información detallada de una fecha específica de crucero.
    /// </summary>
    /// <param name="cruiseDateId">El identificador único de la fecha del crucero.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene los detalles de la fecha del crucero.</returns>
    Task<ApiResult<GetCruiseDateResponse>> GetCruiseDateAsync(int cruiseDateId, CancellationToken ct = default);

    /// <summary>
    /// Recupera la lista de camarotes actualmente bloqueados para una fecha de crucero.
    /// </summary>
    /// <param name="cruiseDateId">El identificador único de la fecha del crucero.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la lista de camarotes bloqueados.</returns>
    Task<ApiResult<List<LockedCabinResponse>>> GetLockedCabinsAsync(int cruiseDateId, CancellationToken ct = default);

    /// <summary>
    /// Bloquea un camarote para la sesión de reserva actual.
    /// </summary>
    /// <param name="cabinId">El identificador único del camarote a bloquear.</param>
    /// <param name="request">La solicitud de bloqueo de camarote que contiene detalles de fecha de crucero y ocupancia.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> LockCabinAsync(int cabinId, LockCabinRequest request, CancellationToken ct = default);

    /// <summary>
    /// Desbloquea un camarote previamente bloqueado.
    /// </summary>
    /// <param name="cabinId">El identificador único del camarote a desbloquear.</param>
    /// <param name="cruiseDateId">El identificador único de la fecha del crucero.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> UnlockCabinAsync(int cabinId, int cruiseDateId, CancellationToken ct = default);

    /// <summary>
    /// Crea una nueva reserva con los camarotes, extras y método de pago especificados.
    /// </summary>
    /// <param name="request">Los detalles de la reserva a crear.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> SaveBookingAsync(BookingDto request, CancellationToken ct = default);

    /// <summary>
    /// Recupera la información de check-in de una reserva usando un token de check-in.
    /// </summary>
    /// <param name="token">El token de check-in para la reserva.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la información de check-in.</returns>
    Task<ApiResult<GetCheckInResponse>> GetCheckInAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Agrega un pasajero a un camarote durante el check-in usando un token de check-in.
    /// </summary>
    /// <param name="token">El token de check-in para la reserva.</param>
    /// <param name="bookingCabinId">El identificador único del camarote dentro de la reserva.</param>
    /// <param name="request">Los detalles del pasajero a agregar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene el identificador del pasajero agregado.</returns>
    Task<ApiResult<AddCheckInPassengerResponse>> AddCheckInPassengerAsync(string token, int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Elimina un pasajero del check-in usando un token de check-in.
    /// </summary>
    /// <param name="token">El token de check-in para la reserva.</param>
    /// <param name="passengerId">El identificador único del pasajero a eliminar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> DeleteCheckInPassengerAsync(string token, int passengerId, CancellationToken ct = default);

    /// <summary>
    /// Completa el proceso de check-in usando un token de check-in.
    /// </summary>
    /// <param name="token">El token de check-in para la reserva.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> CompleteCheckInAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Recupera la información de check-in de una reserva usando el identificador de la reserva.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la información de check-in.</returns>
    Task<ApiResult<GetCheckInResponse>> GetBookingCheckInAsync(int bookingId, CancellationToken ct = default);

    /// <summary>
    /// Agrega un pasajero a un camarote durante el check-in usando el identificador de la reserva.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva.</param>
    /// <param name="bookingCabinId">El identificador único del camarote dentro de la reserva.</param>
    /// <param name="request">Los detalles del pasajero a agregar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene el identificador del pasajero agregado.</returns>
    Task<ApiResult<AddCheckInPassengerResponse>> AddBookingCheckInPassengerAsync(int bookingId, int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Elimina un pasajero del check-in usando el identificador de la reserva.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva.</param>
    /// <param name="passengerId">El identificador único del pasajero a eliminar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> DeleteBookingCheckInPassengerAsync(int bookingId, int passengerId, CancellationToken ct = default);

    /// <summary>
    /// Completa el proceso de check-in usando el identificador de la reserva.
    /// </summary>
    /// <param name="bookingId">El identificador único de la reserva.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> CompleteBookingCheckInAsync(int bookingId, CancellationToken ct = default);
}

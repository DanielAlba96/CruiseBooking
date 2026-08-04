using CruiseBooking.Integrations.Models;

namespace CruiseBooking.Services.Api.CheckIn;

/// <summary>
/// Abstrae los dos métodos para acceder al check-in (por token público o por reserva autenticada)
/// para que las vistas de check-in se puedan compartir entre flujos de check-in basados en token y reserva.
/// </summary>
public interface ICheckInGateway
{
    /// <summary>
    /// Recupera la información de check-in.
    /// </summary>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene la información de check-in.</returns>
    Task<ApiResult<GetCheckInResponse>> Get(CancellationToken ct = default);

    /// <summary>
    /// Agrega un pasajero a un camarote durante el check-in.
    /// </summary>
    /// <param name="bookingCabinId">El identificador único del camarote dentro de la reserva.</param>
    /// <param name="request">Los detalles del pasajero a agregar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que contiene el identificador del pasajero agregado.</returns>
    Task<ApiResult<AddCheckInPassengerResponse>> AddPassenger(int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Elimina un pasajero del check-in.
    /// </summary>
    /// <param name="passengerId">El identificador único del pasajero a eliminar.</param>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> DeletePassenger(int passengerId, CancellationToken ct = default);

    /// <summary>
    /// Completa el proceso de check-in.
    /// </summary>
    /// <param name="ct">Un token de cancelación para la operación.</param>
    /// <returns>Un resultado de API que indica éxito o falla.</returns>
    Task<ApiResult> Complete(CancellationToken ct = default);

    /// <summary>
    /// Obtiene la URL de la página de check-in.
    /// </summary>
    /// <returns>La URL de la página de check-in.</returns>
    string CheckInUrl();

    /// <summary>
    /// Obtiene la URL de la página del formulario de pasajeros.
    /// </summary>
    /// <param name="bookingCabinId">El identificador único del camarote.</param>
    /// <returns>La URL de la página del formulario de pasajeros con el identificador del camarote.</returns>
    string PassengersUrl(int bookingCabinId);

    /// <summary>
    /// Obtiene la URL a la que redirigir después de completar el check-in.
    /// </summary>
    /// <returns>La URL de redireccionamiento después de la finalización, o <see langword="null"/> si no se necesita redireccionamiento.</returns>
    string? CompletedRedirectUrl();
}

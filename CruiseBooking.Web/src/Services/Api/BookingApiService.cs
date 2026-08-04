using System.Runtime.CompilerServices;
using CruiseBooking.Integrations;
using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Dtos;
using Refit;

namespace CruiseBooking.Services.Api;

/// <summary>
/// Implementación de <see cref="IBookingApiService"/> sobre el cliente Refit
/// <see cref="IBookingApi"/>. Todas las llamadas pasan por uno de los dos
/// <c>ExecuteAsync</c> para centralizar el manejo de errores y logging.
/// </summary>
internal sealed class BookingApiService(IBookingApi api, ILogger<BookingApiService> logger) : IBookingApiService
{
    readonly IBookingApi _api = api;
    readonly ILogger<BookingApiService> _logger = logger;

    public Task<ApiResult> CreateOrUpdateUserAsync(CreateOrUpdateUserRequest request, CancellationToken ct = default)
        => ExecuteAsync(token => _api.CreateOrUpdateUser(request, token), ct);

    public Task<ApiResult<List<BookingSummaryResponse>>> GetCurrentUserBookingsAsync(CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetCurrentUserBookings(token), ct);

    public Task<ApiResult<BookingDetailResponse>> GetCurrentUserBookingDetailAsync(int bookingId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetCurrentUserBookingDetail(bookingId, token), ct);

    public Task<ApiResult> CancelCurrentUserBookingAsync(int bookingId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.CancelCurrentUserBooking(bookingId, token), ct);

    public Task<ApiResult> PayBookingAsync(int bookingId, string paymentMethodId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.PayBooking(bookingId, new PayBookingRequest(paymentMethodId), token), ct);

    public Task<ApiResult<List<PaymentMethodResponse>>> GetPaymentMethodsAsync(CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetPaymentMethods(token), ct);

    public Task<ApiResult> DeletePaymentMethodAsync(string id, CancellationToken ct = default)
        => ExecuteAsync(token => _api.DeletePaymentMethod(id, token), ct);

    public Task<ApiResult<AddPaymentMethodResponse>> CreatePaymentSetupIntentAsync(CancellationToken ct = default)
        => ExecuteAsync(token => _api.CreatePaymentSetupIntent(token), ct);

    public Task<ApiResult<GetTaxRateResponse>> GetTaxRateAsync(decimal baseAmount, CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetTaxRate(baseAmount, token), ct);

    public Task<ApiResult<GetCruiseDateResponse>> GetCruiseDateAsync(int cruiseDateId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetCruiseDate(cruiseDateId, token), ct);

    public Task<ApiResult<List<LockedCabinResponse>>> GetLockedCabinsAsync(int cruiseDateId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetLockedCabins(cruiseDateId, token), ct);

    public Task<ApiResult> LockCabinAsync(int cabinId, LockCabinRequest request, CancellationToken ct = default)
        => ExecuteAsync(token => _api.LockCabin(cabinId, request, token), ct);

    public Task<ApiResult> UnlockCabinAsync(int cabinId, int cruiseDateId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.UnlockCabin(cabinId, cruiseDateId, token), ct);

    public Task<ApiResult> SaveBookingAsync(BookingDto request, CancellationToken ct = default)
        => ExecuteAsync(token => _api.SaveBooking(request, token), ct);

    public Task<ApiResult<GetCheckInResponse>> GetCheckInAsync(string token, CancellationToken ct = default)
        => ExecuteAsync(cancellation => _api.GetCheckIn(token, cancellation), ct);

    public Task<ApiResult<AddCheckInPassengerResponse>> AddCheckInPassengerAsync(string token, int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default)
        => ExecuteAsync(cancellation => _api.AddCheckInPassenger(token, bookingCabinId, request, cancellation), ct);

    public Task<ApiResult> DeleteCheckInPassengerAsync(string token, int passengerId, CancellationToken ct = default)
        => ExecuteAsync(cancellation => _api.DeleteCheckInPassenger(token, passengerId, cancellation), ct);

    public Task<ApiResult> CompleteCheckInAsync(string token, CancellationToken ct = default)
        => ExecuteAsync(cancellation => _api.CompleteCheckIn(token, cancellation), ct);

    public Task<ApiResult<GetCheckInResponse>> GetBookingCheckInAsync(int bookingId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.GetBookingCheckIn(bookingId, token), ct);

    public Task<ApiResult<AddCheckInPassengerResponse>> AddBookingCheckInPassengerAsync(int bookingId, int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default)
        => ExecuteAsync(token => _api.AddBookingCheckInPassenger(bookingId, bookingCabinId, request, token), ct);

    public Task<ApiResult> DeleteBookingCheckInPassengerAsync(int bookingId, int passengerId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.DeleteBookingCheckInPassenger(bookingId, passengerId, token), ct);

    public Task<ApiResult> CompleteBookingCheckInAsync(int bookingId, CancellationToken ct = default)
        => ExecuteAsync(token => _api.CompleteBookingCheckIn(bookingId, token), ct);

    async Task<ApiResult> ExecuteAsync(
        Func<CancellationToken, Task<IApiResponse>> call,
        CancellationToken ct,
        [CallerMemberName] string operation = "")
    {
        using var response = await call(ct);

        if (response.StatusCode is null)
        {
            _logger.LogWarning("{Operation}: la API devolvió un código de estado nulo", operation);
            return ApiResult.Failure("La API devolvió un código de estado nulo", 0);
        }

        if (response.HasResponseError(out var apiException))
        {
            var statusCode = (int)apiException.StatusCode;
            _logger.LogWarning(
                apiException,
                "{Operation}: la API devolvió {StatusCode}", operation, statusCode);

            var problemDetail = await apiException.GetContentAsAsync<ProblemDetails>();
            return ApiResult.Failure(problemDetail?.Detail ?? "Error desconocido", statusCode);
        }

        return ApiResult.Success((int)response.StatusCode);
    }

    async Task<ApiResult<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<ApiResponse<T>>> call,
        CancellationToken ct,
        [CallerMemberName] string operation = "")
    {
        using var response = await call(ct);

        if (response.StatusCode is null)    
        {
            _logger.LogWarning("{Operation}: la API devolvió un código de estado nulo", operation);
            return ApiResult<T>.Failure("La API devolvió un código de estado nulo", 0);
        }

        if (response.HasResponseError(out var apiException))
        {
            var statusCode = (int)apiException.StatusCode;
            _logger.LogWarning(apiException,"{Operation}: la API devolvió {StatusCode}", operation, statusCode);

            var problemDetail = await apiException.GetContentAsAsync<ProblemDetails>();
            return ApiResult<T>.Failure(problemDetail?.Detail ?? "Error desconocido", statusCode);
        }

        if (response.Content is null)
        {
            _logger.LogWarning("{Operation}: la API devolvió {StatusCode} con contenido nulo", operation, (int)response.StatusCode);
            return ApiResult<T>.Failure("La API devolvió contenido nulo", (int)response.StatusCode);
        }

        return ApiResult<T>.Success(response.Content, (int)response.StatusCode);
    }
}

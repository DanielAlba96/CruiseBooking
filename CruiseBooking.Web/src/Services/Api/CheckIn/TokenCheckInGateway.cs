using CruiseBooking.Integrations.Models;

namespace CruiseBooking.Services.Api.CheckIn;

/// <summary>
/// Puerta de enlace para operaciones de check-in en una reserva accedida a través de un token de check-in público.
/// </summary>
public class TokenCheckInGateway(IBookingApiService api, string token) : ICheckInGateway
{
    /// <inheritdoc/>
    public Task<ApiResult<GetCheckInResponse>> Get(CancellationToken ct = default)
        => _api.GetCheckInAsync(_token, ct);

    /// <inheritdoc/>
    public Task<ApiResult<AddCheckInPassengerResponse>> AddPassenger(int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default)
        => _api.AddCheckInPassengerAsync(_token, bookingCabinId, request, ct);

    /// <inheritdoc/>
    public Task<ApiResult> DeletePassenger(int passengerId, CancellationToken ct = default)
        => _api.DeleteCheckInPassengerAsync(_token, passengerId, ct);

    /// <inheritdoc/>
    public Task<ApiResult> Complete(CancellationToken ct = default)
        => _api.CompleteCheckInAsync(_token, ct);

    /// <inheritdoc/>
    public string CheckInUrl() => $"/check-in?token={_token}";

    /// <inheritdoc/>
    public string PassengersUrl(int bookingCabinId) => $"/booking-passengers?token={_token}&cabinId={bookingCabinId}";

    /// <inheritdoc/>
    public string? CompletedRedirectUrl() => "/";

    readonly IBookingApiService _api = api;
    readonly string _token = token;
}

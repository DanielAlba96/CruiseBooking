using CruiseBooking.Integrations.Models;

namespace CruiseBooking.Services.Api.CheckIn;

/// <summary>
/// Puerta de enlace para operaciones de check-in en una reserva autenticada.
/// </summary>
public class BookingCheckInGateway(IBookingApiService api, int bookingId) : ICheckInGateway
{
    /// <inheritdoc/>
    public Task<ApiResult<GetCheckInResponse>> Get(CancellationToken ct = default)
        => _api.GetBookingCheckInAsync(_bookingId, ct);

    /// <inheritdoc/>
    public Task<ApiResult<AddCheckInPassengerResponse>> AddPassenger(int bookingCabinId, CheckInPassengerRequest request, CancellationToken ct = default)
        => _api.AddBookingCheckInPassengerAsync(_bookingId, bookingCabinId, request, ct);

    /// <inheritdoc/>
    public Task<ApiResult> DeletePassenger(int passengerId, CancellationToken ct = default)
        => _api.DeleteBookingCheckInPassengerAsync(_bookingId, passengerId, ct);

    /// <inheritdoc/>
    public Task<ApiResult> Complete(CancellationToken ct = default)
        => _api.CompleteBookingCheckInAsync(_bookingId, ct);

    /// <inheritdoc/>
    public string CheckInUrl() => $"/check-in/booking/{_bookingId}";

    /// <inheritdoc/>
    public string PassengersUrl(int bookingCabinId) => $"/booking-passengers/booking/{_bookingId}?cabinId={bookingCabinId}";

    /// <inheritdoc/>
    public string? CompletedRedirectUrl() => "/my-bookings";

    readonly IBookingApiService _api = api;
    readonly int _bookingId = bookingId;
}

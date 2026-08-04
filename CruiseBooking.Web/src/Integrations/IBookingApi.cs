using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Dtos;
using Refit;

namespace CruiseBooking.Integrations;

internal interface IBookingApi
{
    [Post("/user/current")]
    Task<IApiResponse> CreateOrUpdateUser(CreateOrUpdateUserRequest request, CancellationToken cancellationToken = default);
    
    [Post("/booking")]
    Task<IApiResponse> SaveBooking(BookingDto request, CancellationToken cancellationToken = default);
    
    [Get("/booking/list")]
    Task<ApiResponse<List<BookingSummaryResponse>>> GetCurrentUserBookings(CancellationToken cancellationToken = default);

    [Get("/booking/{bookingId}")]
    Task<ApiResponse<BookingDetailResponse>> GetCurrentUserBookingDetail(int bookingId, CancellationToken cancellationToken = default);

    [Delete("/booking/{bookingId}")]
    Task<IApiResponse> CancelCurrentUserBooking(int bookingId, CancellationToken cancellationToken = default);

    [Post("/booking/{bookingId}/pay")]
    Task<IApiResponse> PayBooking(int bookingId, PayBookingRequest request, CancellationToken cancellationToken = default);

    [Get("/payment/method/list")]
    Task<ApiResponse<List<PaymentMethodResponse>>> GetPaymentMethods(CancellationToken cancellationToken = default);

    [Delete("/payment/method")]
    Task<IApiResponse> DeletePaymentMethod(string paymentMethodId, CancellationToken cancellationToken = default);

    [Post("/payment/method")]
    Task<ApiResponse<AddPaymentMethodResponse>> CreatePaymentSetupIntent(CancellationToken cancellationToken = default);

    [Get("/payment/tax")]
    Task<ApiResponse<GetTaxRateResponse>> GetTaxRate(decimal baseAmount, CancellationToken cancellationToken = default);

    [Get("/cruise-date/{cruiseDateId}")]
    Task<ApiResponse<GetCruiseDateResponse>> GetCruiseDate(int cruiseDateId, CancellationToken cancellationToken = default);

    [Get("/cabin/lock/list")]
    Task<ApiResponse<List<LockedCabinResponse>>> GetLockedCabins(int cruiseDateId, CancellationToken cancellationToken = default);

    [Put("/cabin/{cabinId}/lock")]
    Task<IApiResponse> LockCabin(int cabinId, LockCabinRequest request, CancellationToken cancellationToken = default);

    [Delete("/cabin/{cabinId}/lock")]
    Task<IApiResponse> UnlockCabin(int cabinId, int cruiseDateId,CancellationToken cancellationToken = default);

    [Get("/check-in/{token}")]
    Task<ApiResponse<GetCheckInResponse>> GetCheckIn(string token, CancellationToken cancellationToken = default);

    [Post("/check-in/{token}/cabins/{bookingCabinId}/passengers")]
    Task<ApiResponse<AddCheckInPassengerResponse>> AddCheckInPassenger(string token, int bookingCabinId, CheckInPassengerRequest request, CancellationToken cancellationToken = default);

    [Delete("/check-in/{token}/passengers/{passengerId}")]
    Task<IApiResponse> DeleteCheckInPassenger(string token, int passengerId, CancellationToken cancellationToken = default);

    [Post("/check-in/{token}/complete")]
    Task<IApiResponse> CompleteCheckIn(string token, CancellationToken cancellationToken = default);

    [Get("/check-in/booking/{bookingId}")]
    Task<ApiResponse<GetCheckInResponse>> GetBookingCheckIn(int bookingId, CancellationToken cancellationToken = default);

    [Post("/check-in/booking/{bookingId}/cabins/{bookingCabinId}/passengers")]
    Task<ApiResponse<AddCheckInPassengerResponse>> AddBookingCheckInPassenger(int bookingId, int bookingCabinId, CheckInPassengerRequest request, CancellationToken cancellationToken = default);

    [Delete("/check-in/booking/{bookingId}/passengers/{passengerId}")]
    Task<IApiResponse> DeleteBookingCheckInPassenger(int bookingId, int passengerId, CancellationToken cancellationToken = default);

    [Post("/check-in/booking/{bookingId}/complete")]
    Task<IApiResponse> CompleteBookingCheckIn(int bookingId, CancellationToken cancellationToken = default);
}

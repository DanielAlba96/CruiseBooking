using CruiseBooking.Components.Shared;
using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Api;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;
using System.Globalization;
using System.Net;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Muestra las reservas del usuario actual con opciones para ver detalles y cancelar reservaciones.
/// </summary>
public partial class MyBookings(
    IBookingApiService api,
    NavigationManager navigationManager,
    IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
    ISnackbar snackbar,
    IDialogService dialogService)
{
    static readonly CultureInfo PriceCulture = CultureInfo.GetCultureInfo("es-ES");

    readonly IBookingApiService _api = api;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly IOptionsSnapshot<OpenIdConnectOptions> _oidcOptions = oidcOptions;
    readonly ISnackbar _snackbar = snackbar;
    readonly IDialogService _dialogService = dialogService;

    readonly List<BookingSummaryResponse> _bookings = [];

    BookingDetailResponse? _selectedBooking;
    int? _selectedBookingId;

    bool _isLoadingBookings = true;
    bool _bookingsLoadError;
    string _bookingsErrorMessage = string.Empty;

    bool _isLoadingDetail;
    bool _isCancelingBooking;
    bool _detailLoadError;
    bool _detailNotFound;
    bool _isDetailUserNotRegistered;
    string _detailErrorMessage = string.Empty;

    decimal _baseAmount;
    decimal _totalAmount;
    decimal _taxAmount;
    string _taxRate = string.Empty;

    string _keycloakAccountUrl = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authUrl = _oidcOptions.Get(OpenIdConnectDefaults.AuthenticationScheme).Authority;
        var returnUrl = _navigationManager.BaseUri + "my-bookings";
        _keycloakAccountUrl = $"{authUrl}/account?referrer=cruises-ui&referrer_uri={returnUrl}";

        await LoadBookingsAsync();
    }

    async Task LoadBookingsAsync()
    {
        _isLoadingBookings = true;
        _bookingsLoadError = false;
        _bookingsErrorMessage = string.Empty;

        try
        {
            var result = await _api.GetCurrentUserBookingsAsync();

            if (result.IsUnauthorized || result.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                NavigateToLogin();
                return;
            }

            if (result.IsSuccess)
            {
                _bookings.Clear();
                _bookings.AddRange(result.Value!);

                if (_selectedBookingId is not null && _bookings.All(x => x.Id != _selectedBookingId.Value))
                {
                    ClearDetailSelection();
                }

                return;
            }

            _bookingsLoadError = true;
            _bookingsErrorMessage = "No se pudo cargar el listado de reservas. Intentalo de nuevo en unos minutos.";
        }
        finally
        {
            _isLoadingBookings = false;
        }
    }

    async Task SelectBookingAsync(int bookingId)
    {
        if (_selectedBookingId == bookingId)
        {
            ClearDetailSelection();
            return;
        }

        _selectedBookingId = bookingId;
        _selectedBooking = null;
        _isLoadingDetail = true;
        _detailLoadError = false;
        _detailNotFound = false;
        _isDetailUserNotRegistered = false;
        _detailErrorMessage = string.Empty;
        _baseAmount = 0;
        _totalAmount = 0;
        _taxAmount = 0;
        _taxRate = string.Empty;

        try
        {
            var result = await _api.GetCurrentUserBookingDetailAsync(bookingId);

            if (result.IsUnauthorized)
            {
                NavigateToLogin();
                return;
            }

            if (result.IsSuccess)
            {
                _selectedBooking = result.Value;
                await LoadBookingTaxAsync(result.Value!);
                return;
            }

            if (result.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                _isDetailUserNotRegistered = true;
                return;
            }

            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _detailNotFound = true;
                return;
            }

            _detailLoadError = true;
            _detailErrorMessage = "No se pudo cargar el detalle de la reserva seleccionada.";
        }
        finally
        {
            _isLoadingDetail = false;
        }
    }

    async Task LoadBookingTaxAsync(BookingDetailResponse booking)
    {
        var baseAmount = booking.Cabins.Sum(GetCabinPrice) + booking.Extras.Sum(e => e.Price);
        var result = await _api.GetTaxRateAsync(baseAmount);

        if (result.IsUnauthorized)
        {
            NavigateToLogin();
            return;
        }

        if (!result.IsSuccess || result.Value is not { } tax)
        {
            _detailLoadError = true;
            _detailErrorMessage = "No se pudo cargar la información de impuestos de la reserva.";
            return;
        }

        _baseAmount = baseAmount;
        _totalAmount = tax.TotalAmount;
        _taxAmount = _totalAmount - _baseAmount;
        _taxRate = tax.TaxPercentage;
    }

    Task RetryLoadBookingsAsync() => LoadBookingsAsync();

    Task RetrySelectedBookingAsync()
        => _selectedBookingId is int bookingId
            ? SelectBookingAsync(bookingId)
            : Task.CompletedTask;

    async Task CancelSelectedBookingAsync()
    {
        if (_isCancelingBooking || _selectedBookingId is not int bookingId
            || (_selectedBooking is not null && !CanCancelBooking(_selectedBooking)))
        {
            return;
        }

        var parameters = new DialogParameters<CancelBookingDialog>
        {
            { x => x.BookingName, _selectedBooking?.Name ?? string.Empty },
            { x => x.BookingCode, _selectedBooking?.Code ?? string.Empty },
            { x => x.StartDate, _selectedBooking is null ? string.Empty : FormatDate(_selectedBooking.StartDate) },
            { x => x.EndDate, _selectedBooking is null ? string.Empty : FormatDate(_selectedBooking.EndDate) }
        };

        var dialog = await _dialogService.ShowAsync<CancelBookingDialog>(
            title: "Cancelar reserva",
            parameters: parameters,
            options: new DialogOptions
            {
                CloseOnEscapeKey = true,
                CloseButton = true,
                BackdropClick = false,
                MaxWidth = MaxWidth.ExtraSmall,
                FullWidth = true
            });

        var dialogResult = await dialog.Result;

        if (dialogResult is null || dialogResult.Canceled)
        {
            return;
        }

        _isCancelingBooking = true;

        try
        {
            var result = await _api.CancelCurrentUserBookingAsync(bookingId);

            if (result.IsUnauthorized)
            {
                NavigateToLogin();
                return;
            }

            if (!result.IsSuccess)
            {
                _snackbar.Add(result.ErrorMessage!, Severity.Error);
                return;
            }

            _snackbar.Add("La reserva se ha cancelado correctamente. En breve recibirás el reembolso en el método de pago vinculado a tu cuenta", Severity.Success);
            await LoadBookingsAsync();
            await SelectBookingAsync(bookingId);
        }
        finally
        {
            _isCancelingBooking = false;
        }
    }

    void ClearDetailSelection()
    {
        _selectedBookingId = null;
        _selectedBooking = null;
        _isLoadingDetail = false;
        _isCancelingBooking = false;
        _detailLoadError = false;
        _detailNotFound = false;
        _isDetailUserNotRegistered = false;
        _detailErrorMessage = string.Empty;
        _baseAmount = 0;
        _totalAmount = 0;
        _taxAmount = 0;
        _taxRate = string.Empty;
    }

    string GetBookingCardStyle(int bookingId)
    {
        var isSelected = _selectedBookingId == bookingId;
        return isSelected
            ? "cursor: pointer; border: 2px solid #72ace6; background-color: rgba(114, 172, 230, 0.12);"
            : "cursor: pointer; border: 1px solid #dde4ef;";
    }

    static bool CanCancelBooking(BookingDetailResponse booking)
        => booking.Status != BookingStatus.Cancelada && booking.CheckInStartDate > DateTime.UtcNow;

    static Color GetStatusColor(BookingStatus status) => status switch
    {
        BookingStatus.PagoPendiente => Color.Warning,
        BookingStatus.Cancelada => Color.Error,
        _ => Color.Success
    };

    static string GetStatusStyle(BookingStatus status)
        => status == BookingStatus.CheckinPendiente
            ? "background-color: #fbc02d; color: #000;"
            : string.Empty;

    static string GetStatusText(BookingStatus status) => status switch
    {
        BookingStatus.Creada => "Creada",
        BookingStatus.PagoPendiente => "Pago pendiente",
        BookingStatus.CheckinPendiente => "Checkin pendiente",
        BookingStatus.Completada => "Completada",
        BookingStatus.Cancelada => "Cancelada",
        _ => status.ToString()
    };

    static decimal GetCabinPrice(BookingDetailCabinResponse cabin)
        => cabin.Price != 0 ? cabin.Price : cabin.CurrentPrice;

    static string FormatDate(DateTime value) => value.ToLocalTime().ToString("dd/MM/yyyy");

    static string FormatDateTime(DateTime value) => value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    static string FormatNullableDateTime(DateTime? value)
        => value is null ? "Pendiente" : FormatDateTime(value.Value);

    static string FormatPrice(decimal value) => value.ToString("C", PriceCulture);

    void NavigateToLogin()
    {
        var relative = $"/{_navigationManager.ToBaseRelativePath(_navigationManager.Uri)}";
        var returnUrl = string.IsNullOrWhiteSpace(relative) || relative == "/" ? "/my-bookings" : relative;
        var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _navigationManager.NavigateTo(loginUrl, forceLoad: true);
    }
}

using CruiseBooking.Components.Shared;
using CruiseBooking.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Habilita el procesamiento de pago manual para una reserva pendiente.
/// </summary>
public partial class PayBooking(
    IBookingApiService api,
    NavigationManager navigationManager,
    ISnackbar snackbar)
{
    readonly IBookingApiService _api = api;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece el identificador único de la reserva a pagar.
    /// </summary>
    [Parameter] public int BookingId { get; set; }

    PaymentMethodSelector? _paymentSelector;
    bool _isLoading;

    async Task CompletePayment()
    {
        if (_isLoading || _paymentSelector is null)
            return;

        _isLoading = true;
        try
        {
            var paymentMethodId = await _paymentSelector.ResolvePaymentMethodIdAsync();
            if (string.IsNullOrEmpty(paymentMethodId))
                return;

            var result = await _api.PayBookingAsync(BookingId, paymentMethodId);
            if (result.IsUnauthorized)
            {
                NavigateToLogin();
                return;
            }

            if (!result.IsSuccess)
            {
                _snackbar.Add(result.ErrorMessage!, Severity.Error);
                await _paymentSelector.HandleSaveFailureAsync();
                return;
            }

            _snackbar.Add("¡Reserva pagada con éxito! En breve recibirá la factura en el correo vinculado a su cuenta", Severity.Success);
            _navigationManager.NavigateTo("/my-bookings");
        }
        finally
        {
            _isLoading = false;
        }
    }

    void NavigateBack() => _navigationManager.NavigateTo("/my-bookings");

    void NavigateToLogin()
    {
        var relative = $"/{_navigationManager.ToBaseRelativePath(_navigationManager.Uri)}";
        var returnUrl = string.IsNullOrWhiteSpace(relative) || relative == "/" ? "/my-bookings" : relative;
        var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _navigationManager.NavigateTo(loginUrl, forceLoad: true);
    }
}

using CruiseBooking.Components.Shared;
using CruiseBooking.Services;
using CruiseBooking.Services.Api;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Muestra el resumen de la reserva con información de precios y procesamiento de pagos.
/// </summary>
public partial class BookingSummary(
    BookingStateService bookingContainer,
    NavigationManager navigationManager,
    IBookingApiService api,
    ISnackbar snackbar)
{
    readonly BookingStateService _bookingContainer = bookingContainer;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly IBookingApiService _api = api;
    readonly ISnackbar _snackbar = snackbar;

    PaymentMethodSelector? _paymentSelector;

    bool _isLoading = false;
    bool _loadError = false;
    string _errorMessage = string.Empty;

    decimal _baseAmount;
    decimal _totalAmount;
    decimal _taxAmount;
    string _taxRate = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (_bookingContainer.Cruise is null)
            {
                _snackbar.Add("No hay crucero seleccionado. Por favor, selecciona uno nuevamente.", MudBlazor.Severity.Warning);
                _navigationManager.NavigateTo("/cruise-search");
                return;
            }

            await LoadAsync();
        }
        catch
        {
            _loadError = true;
            _errorMessage = "Error al cargar la información de la reserva. Por favor, inténtalo de nuevo.";
        }
    }

    async Task LoadAsync()
    {
        _loadError = false;

        var baseAmount = _bookingContainer.SelectedCabins.Sum(c => c.CurrentPrice) + _bookingContainer.SelectedExtras.Sum(e => e.Price);
        var taxResult = await _api.GetTaxRateAsync(baseAmount);

        if (taxResult.IsUnauthorized)
        {
            NavigateToLogin();
            return;
        }

        if (!taxResult.IsSuccess || taxResult.Value is not { } tax)
        {
            _loadError = true;
            _errorMessage = "No se pudo cargar la información de impuestos. Por favor, inténtalo de nuevo.";
            return;
        }

        _baseAmount = baseAmount;
        _totalAmount = tax.TotalAmount;
        _taxAmount = _totalAmount - _baseAmount;
        _taxRate = tax.TaxPercentage;
    }

    async Task CompleteBooking()
    {
        if (_isLoading || _paymentSelector is null)
            return;

        _isLoading = true;
        try
        {
            var paymentMethodId = await _paymentSelector.ResolvePaymentMethodIdAsync();
            if (string.IsNullOrEmpty(paymentMethodId))
            {
                _isLoading = false;
                return;
            }
            var bookingResult = await _api.SaveBookingAsync(_bookingContainer.GenerateBookingDto(paymentMethodId));
            if (bookingResult.IsUnauthorized)
            {
                NavigateToLogin();
                return;
            }

            if (!bookingResult.IsSuccess)
            {
                _snackbar.Add(bookingResult.ErrorMessage!, MudBlazor.Severity.Error);
                await _paymentSelector.HandleSaveFailureAsync();
                _isLoading = false;
                return;
            }

            _snackbar.Add("¡Reserva completada con éxito! Puede realizar el pago de forma anticipada desde Mis Reservas o esperar al pago automático.", MudBlazor.Severity.Success);

            _bookingContainer.Clear();
            _navigationManager.NavigateTo("/");
        }
        finally
        {
            _isLoading = false;
        }
    }

    async Task RetryLoadAsync()
    {
        await LoadAsync();
        StateHasChanged();
    }

    void NavigateBackToCabins()
    {
        var cruise = _bookingContainer.Cruise!;
        _navigationManager.NavigateTo($"/book-cruise?cruiseId={cruise.CruiseId}&cruiseDateId={cruise.CruiseDateId}");
    }

    void NavigateToLogin()
    {
        var relative = $"/{_navigationManager.ToBaseRelativePath(_navigationManager.Uri)}";
        var returnUrl = string.IsNullOrWhiteSpace(relative) || relative == "/" ? "/booking-summary" : relative;
        var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _navigationManager.NavigateTo(loginUrl, forceLoad: true);
    }
}

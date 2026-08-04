using CruiseBooking.Integrations.Models;
using CruiseBooking.Services;
using CruiseBooking.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Componente para seleccionar o ingresar un método de pago usando integración de Stripe.
/// </summary>
public partial class PaymentMethodSelector(
    IBookingApiService api,
    ISnackbar snackbar,
    NavigationManager navigationManager,
    IOptions<StripeOptions> stripeOptions) : IAsyncDisposable
{
    const string StripeModulePath = "./js/stripeInterop.js";

    /// <summary>
    /// Obtiene o establece un valor que indica si se debe mostrar el botón para guardar una tarjeta nueva.
    /// </summary>
    [Parameter] public bool ShowSaveNewCardButton { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si se deben mostrar sugerencias para el flujo de pago-reserva.
    /// </summary>
    [Parameter] public bool ShowPayBookingHint { get; set; } = true;

    /// <summary>
    /// Obtiene o establece un valor que indica si se debe mostrar como selector o en línea.
    /// </summary>
    [Parameter] public bool ShowAsSelector { get; set; } = true;

    /// <summary>
    /// Obtiene o establece una devolución de llamada que se invoca cuando cambia la selección del método de pago o el estado de disponibilidad.
    /// </summary>
    [Parameter] public EventCallback OnStateChanged { get; set; }

    readonly IBookingApiService _api = api;
    readonly ISnackbar _snackbar = snackbar;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly string _publishableKey = stripeOptions.Value.PublishableKey;

    IJSObjectReference? _module;

    List<PaymentMethodResponse> _paymentMethods = [];
    string? _selectedPaymentMethodId;
    string? _deletingPaymentMethodId;

    bool _useNewCard;

    string? _clientSecret;
    string? _stripeError;
    bool _stripeReady;
    bool _needsMount;
    bool _isSavingNewCard;

    sealed record ConfirmSetupResult(string? PaymentMethodId, string? Error);

    /// <summary>
    /// Gets a value that indicates whether a payment method is ready for use.
    /// </summary>
    public bool IsReady => _useNewCard ? _stripeReady : !string.IsNullOrEmpty(_selectedPaymentMethodId);

    protected override async Task OnInitializedAsync()
    {
        if (!await LoadPaymentMethodsAsync())
            return;

        if (_useNewCard)
        {
            await EnsureSetupIntentAsync();
            _needsMount = _clientSecret is not null;
        }

        await NotifyStateChangedAsync();
    }

    Task NotifyStateChangedAsync() => OnStateChanged.InvokeAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_needsMount && _clientSecret is not null)
        {
            _needsMount = false;
            await InitStripeAsync();
        }
    }

    async Task<bool> LoadPaymentMethodsAsync()
    {
        var result = await _api.GetPaymentMethodsAsync();
        if (result.IsUnauthorized)
        {
            NavigateToLogin();
            return false;
        }

        if (result.IsSuccess)
            _paymentMethods = [.. result.Value!.Where(m => m.Type == PaymentMethodType.Card)];
        else
            _snackbar.Add("No se pudieron cargar tus métodos de pago. Tendrás que introducir una tarjeta.", Severity.Warning);

        _selectedPaymentMethodId = _paymentMethods.FirstOrDefault()?.Id;
        _useNewCard = _paymentMethods.Count == 0;
        return true;
    }

    async Task<bool> EnsureSetupIntentAsync()
    {
        if (_clientSecret is not null)
            return true;

        var result = await _api.CreatePaymentSetupIntentAsync();
        if (result.IsUnauthorized)
        {
            NavigateToLogin();
            return false;
        }

        if (!result.IsSuccess || string.IsNullOrEmpty(result.Value?.ClientSecret))
        {
            _stripeError = "No se pudo inicializar el pago. Por favor, inténtalo de nuevo.";
            return false;
        }

        _clientSecret = result.Value.ClientSecret;
        return true;
    }

    async Task InitStripeAsync()
    {
        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", StripeModulePath);

            var error = await _module.InvokeAsync<string?>("initialize", _publishableKey, _clientSecret);
            if (!string.IsNullOrEmpty(error))
            {
                _stripeError = error;
                await InvokeAsync(StateHasChanged);
                return;
            }

            _stripeReady = true;
            await InvokeAsync(StateHasChanged);
            await NotifyStateChangedAsync();
        }
        catch (JSDisconnectedException) { }
        catch
        {
            _stripeError = "No se pudo cargar el formulario de pago.";
            await InvokeAsync(StateHasChanged);
        }
    }

    async Task ShowNewCardAsync()
    {
        _useNewCard = true;
        _stripeError = null;
        _stripeReady = false;
        await EnsureSetupIntentAsync();
        _needsMount = _clientSecret is not null;
        await NotifyStateChangedAsync();
    }

    async Task CancelNewCardAsync()
    {
        _useNewCard = false;
        _stripeError = null;
        _stripeReady = false;
        _needsMount = false;

        try
        {
            if (_module is not null)
                await _module.InvokeVoidAsync("destroy");
        }
        catch (JSDisconnectedException) { }

        await NotifyStateChangedAsync();
    }

    Task OnSelectedPaymentMethodChangedAsync() => NotifyStateChangedAsync();

    async Task DeletePaymentMethodAsync(string paymentMethodId)
    {
        if (_deletingPaymentMethodId is not null)
            return;

        _deletingPaymentMethodId = paymentMethodId;
        try
        {
            var result = await _api.DeletePaymentMethodAsync(paymentMethodId);

            if (result.IsUnauthorized)
            {
                NavigateToLogin();
                return;
            }

            if (!result.IsSuccess)
            {
                _snackbar.Add(
                    result.ErrorMessage!,
                    Severity.Error);
                return;
            }

            _paymentMethods.RemoveAll(m => m.Id == paymentMethodId);

            if (_selectedPaymentMethodId == paymentMethodId)
                _selectedPaymentMethodId = _paymentMethods.FirstOrDefault()?.Id;

            _snackbar.Add("Método de pago eliminado.", Severity.Success);

            if (_paymentMethods.Count == 0)
                await ShowNewCardAsync();

            await NotifyStateChangedAsync();
        }
        finally
        {
            _deletingPaymentMethodId = null;
        }
    }

    /// <summary>
    /// Determina el medio de paso que se usará en la reserva.
    /// </summary>
    /// <returns>El medio de pago</returns>
    public async Task<string?> ResolvePaymentMethodIdAsync()
    {
        _stripeError = null;
        string? paymentMethodId = _selectedPaymentMethodId;

        if (_useNewCard)
        {
            if (_module is null || !_stripeReady)
                return null;

            var setup = await _module.InvokeAsync<ConfirmSetupResult>("confirmSetup");
            if (!string.IsNullOrEmpty(setup.Error))
            {
                _stripeError = setup.Error;
                return null;
            }

            if (string.IsNullOrEmpty(setup.PaymentMethodId))
            {
                _stripeError = "No se pudo procesar la tarjeta. Por favor, inténtalo de nuevo.";
                return null;
            }

            _clientSecret = null;
            paymentMethodId = setup.PaymentMethodId;
        }

        if (string.IsNullOrEmpty(paymentMethodId))
        {
            _snackbar.Add("Selecciona un método de pago para continuar.", Severity.Warning);
            return null;
        }

        return paymentMethodId;
    }

    /// <summary>
    /// Saves a new card as a payment method without associating it to a booking.
    /// </summary>
    async Task SaveNewCardAsync()
    {
        if (_isSavingNewCard)
            return;

        _isSavingNewCard = true;
        try
        {
            var paymentMethodId = await ResolvePaymentMethodIdAsync();
            if (paymentMethodId is null)
                return;

            await CancelNewCardAsync();
            await LoadPaymentMethodsAsync();
            _snackbar.Add("Tarjeta guardada correctamente.", Severity.Success);
            await NotifyStateChangedAsync();
        }
        finally
        {
            _isSavingNewCard = false;
        }
    }

    /// <summary>
    /// Handles recovery when saving or charging a new card fails.
    /// </summary>
    public async Task HandleSaveFailureAsync()
    {
        if (!_useNewCard)
            return;

        _clientSecret = null;
        _stripeReady = false;
        await EnsureSetupIntentAsync();
        _needsMount = _clientSecret is not null;
    }

    static string DescribePaymentMethod(PaymentMethodResponse method)
    {
        var card = method.Card;
        if (card is null)
            return "Tarjeta guardada";

        return $"•••• {card.Last4} · Caduca {card.ExpMonth:00}/{card.ExpYear}";
    }

    void NavigateToLogin()
    {
        var relative = $"/{_navigationManager.ToBaseRelativePath(_navigationManager.Uri)}";
        var returnUrl = string.IsNullOrWhiteSpace(relative) || relative == "/" ? "/" : relative;
        var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _navigationManager.NavigateTo(loginUrl, forceLoad: true);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_module is not null)
                await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
    }
}

using CruiseBooking.Services.Api;
using CruiseBooking.Services.Api.CheckIn;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Inicia el proceso de check-in usando un token de check-in público.
/// </summary>
public partial class CheckIn(IBookingApiService api, NavigationManager navigationManager, ISnackbar snackbar)
{
    readonly IBookingApiService _api = api;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece el token de check-in público para la reserva.
    /// </summary>
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    ICheckInGateway? _gateway;

    protected override Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            _snackbar.Add("Enlace de check-in inválido.", Severity.Warning);
            _navigationManager.NavigateTo("/");
            return Task.CompletedTask;
        }

        _gateway = new TokenCheckInGateway(_api, Token);
        return Task.CompletedTask;
    }
}

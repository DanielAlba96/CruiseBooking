using CruiseBooking.Services.Api;
using CruiseBooking.Services.Api.CheckIn;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Recopila información de pasajeros para un camarote durante el check-in usando un token.
/// </summary>
public partial class PassengersForm(
    IBookingApiService api,
    NavigationManager navigationManager,
    ISnackbar snackbar)
{
    readonly IBookingApiService _api = api;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece el token de check-in público para la reserva.
    /// </summary>
    [SupplyParameterFromQuery(Name = "token")]
    public string? Token { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador único del camarote para la entrada de pasajeros.
    /// </summary>
    [SupplyParameterFromQuery(Name = "cabinId")]
    public int? CabinId { get; set; }

    ICheckInGateway? _gateway;

    protected override void OnInitialized()
    {
        if (string.IsNullOrWhiteSpace(Token) || CabinId is not > 0)
        {
            _snackbar.Add("Enlace de check-in inválido.", Severity.Warning);
            _navigationManager.NavigateTo("/");
            return;
        }

        _gateway = new TokenCheckInGateway(_api, Token);
    }
}

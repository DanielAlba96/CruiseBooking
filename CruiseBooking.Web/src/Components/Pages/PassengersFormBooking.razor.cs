using CruiseBooking.Services.Api;
using CruiseBooking.Services.Api.CheckIn;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Recopila información de pasajeros para un camarote durante el check-in usando una reserva autenticada.
/// </summary>
public partial class PassengersFormBooking(
    IBookingApiService api,
    NavigationManager navigationManager,
    ISnackbar snackbar)
{
    readonly IBookingApiService _api = api;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece el identificador único de la reserva.
    /// </summary>
    [Parameter]
    public int BookingId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador único del camarote para la entrada de pasajeros.
    /// </summary>
    [SupplyParameterFromQuery(Name = "cabinId")]
    public int? CabinId { get; set; }

    ICheckInGateway? _gateway;

    protected override void OnParametersSet()
    {
        if (CabinId is not > 0)
        {
            _snackbar.Add("Enlace de check-in inválido.", Severity.Warning);
            _navigationManager.NavigateTo("/");
            return;
        }

        _gateway = new BookingCheckInGateway(_api, BookingId);
    }
}

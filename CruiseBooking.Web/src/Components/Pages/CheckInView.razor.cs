using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Api.CheckIn;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Muestra la vista de check-in permitiendo agregar o quitar pasajeros de los camarotes.
/// </summary>
public partial class CheckInView(NavigationManager navigationManager, ISnackbar snackbar)
{
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece la puerta de enlace de check-in que maneja las operaciones de API.
    /// </summary>
    [Parameter, EditorRequired]
    public ICheckInGateway Gateway { get; set; } = null!;

    GetCheckInResponse? _checkIn;
    bool _isLoading = true;
    bool _loadError;
    string _errorMessage = string.Empty;
    int? _removingPassengerId;
    bool _completing;

    bool AllCabinsFull => _checkIn is { Cabins.Count: > 0 }
        && _checkIn.Cabins.All(c => c.Passengers.Count == c.Occupancy);

    string? _loadedKey;

    protected override async Task OnParametersSetAsync()
    {
        var key = Gateway.CheckInUrl();
        if (_loadedKey == key)
            return;

        _loadedKey = key;
        await LoadAsync();
    }

    async Task LoadAsync()
    {
        _isLoading = true;
        _loadError = false;
        try
        {
            var result = await Gateway.Get();
            if (!result.IsSuccess)
            {
                _loadError = true;
                _errorMessage = "No se pudo cargar el check-in. Por favor, inténtalo de nuevo.";
                return;
            }

            _checkIn = result.Value;
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

    async Task CompleteCheckInAsync()
    {
        _completing = true;
        try
        {
            var result = await Gateway.Complete();
            if (!result.IsSuccess)
            {
                _snackbar.Add(result.ErrorMessage!, Severity.Error);
                return;
            }

            _snackbar.Add("Check-in finalizado correctamente.", Severity.Success);

            var redirectUrl = Gateway.CompletedRedirectUrl();
            if (redirectUrl is not null)
            {
                _navigationManager.NavigateTo(redirectUrl);
                return;
            }

            await LoadAsync();
        }
        finally
        {
            _completing = false;
        }
    }

    async Task RemovePassengerAsync(int passengerId)
    {
        _removingPassengerId = passengerId;
        try
        {
            var result = await Gateway.DeletePassenger(passengerId);
            if (!result.IsSuccess)
            {
                _snackbar.Add(result.ErrorMessage!, Severity.Error);
                return;
            }

            await LoadAsync();
        }
        finally
        {
            _removingPassengerId = null;
        }
    }
}

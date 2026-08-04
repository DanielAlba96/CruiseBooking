using CruiseBooking.Integrations.Models;
using CruiseBooking.Services;
using CruiseBooking.Services.Api;
using CruiseBooking.Vms;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Permite a los usuarios seleccionar camarotes y extras para una reserva de crucero.
/// </summary>
public partial class BookCruise(IBookingApiService api, BookingStateService bookingContainer, NavigationManager navigationManager, ISnackbar snackbar)
{
    readonly IBookingApiService _api = api;
    readonly BookingStateService _bookingContainer = bookingContainer;
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece el identificador único del crucero.
    /// </summary>
    [SupplyParameterFromQuery(Name = "cruiseId")]
    public int CruiseId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador único de la fecha del crucero.
    /// </summary>
    [SupplyParameterFromQuery(Name = "cruiseDateId")]
    public int CruiseDateId { get; set; }

    List<CabinSelectionVm> _availableCabins = [];
    List<ExtraSelectionVm> _availableExtras = [];

    /// <summary>
    /// Obtiene o establece la información del crucero seleccionado.
    /// </summary>
    public CruiseSelectionVm? Cruise { get; set; }

    /// <summary>
    /// Obtiene o establece la lista de camarotes seleccionados para esta reserva.
    /// </summary>
    public List<CabinSelectionVm> SelectedCabins { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la lista de extras seleccionados para esta reserva.
    /// </summary>
    public List<ExtraSelectionVm> SelectedExtras { get; set; } = [];

    bool _loadError = false;
    string _errorMessage = string.Empty;
    bool _isLocking = false;

    /// <summary>
    /// Obtiene el precio total de la reserva incluyendo camarotes y extras.
    /// </summary>
    public decimal TotalPrice =>
        SelectedCabins.Sum(c => c.CurrentPrice) +
        SelectedExtras.Sum(e => e.Price);

    protected override async Task OnInitializedAsync()
    {
        if (_bookingContainer.Cruise?.CruiseDateId != CruiseDateId)
            _bookingContainer.Clear();

        if (CruiseId == 0 || CruiseDateId == 0)
        {
            _snackbar.Add("Parámetros de búsqueda inválidos. Por favor, selecciona un crucero nuevamente.", Severity.Warning);
            _navigationManager.NavigateTo("/cruise-search");
            return;
        }

        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            _loadError = false;
            var result = await _api.GetCruiseDateAsync(CruiseDateId);
            if (!result.IsSuccess || result.Value is not { } data)
            {
                _loadError = true;
                _errorMessage = "No se pudo cargar los detalles del crucero. Por favor, inténtalo de nuevo.";
                return;
            }

            var lockedCabinsResult = await _api.GetLockedCabinsAsync(CruiseDateId);
            if (!lockedCabinsResult.IsSuccess || lockedCabinsResult.Value is not { } lockedCabins)
            {
                _loadError = true;
                _errorMessage = "No se pudo cargar los camarotes bloqueados. Por favor, inténtalo de nuevo.";
                return;
            }

            Cruise = new CruiseSelectionVm
            {
                CruiseId = CruiseId,
                CruiseDateId = CruiseDateId,
                Name = data.Name,
                OriginPort = data.OriginPort,
                StartDate = data.StartDate,
                DurationInDays = data.DurationInDays
            };

            SelectedCabins = [.. lockedCabins
                .Join(data.AvailableCabins, l => l.CabinId, c => c.Id, (l, c) => new CabinSelectionVm
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Number = c.AvailableCount,
                    MaxOccupancy = c.MaxOccupancy,
                    CurrentOccupancy = l.Occupants,
                    FullPrice = c.FullPrice
                })];
            _bookingContainer.UpdateCabins(SelectedCabins);
            SelectedExtras = [.. _bookingContainer.SelectedExtras];

            // Todo camarote que ya está en el estado de la reserva está bloqueado en el backend,
            // así que AvailableCount ya lo excluye. Se vuelve a sumar aquí porque
            // GetRemainingCabins resta todos los seleccionados: de lo contrario se descontarían
            // dos veces al recargar la página.
            var lockedCountByCabinId = SelectedCabins
                .GroupBy(c => c.Id)
                .ToDictionary(g => g.Key, g => g.Count());

            _availableCabins = [.. data.AvailableCabins.Select(c => new CabinSelectionVm
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Number = c.AvailableCount + lockedCountByCabinId.GetValueOrDefault(c.Id),
                MaxOccupancy = c.MaxOccupancy,
                CurrentOccupancy = 1,
                FullPrice = c.FullPrice
            })];

            _availableExtras = [.. data.AvailableExtras.Select(e => new ExtraSelectionVm
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price
            })];
        }
        catch
        {
            _loadError = true;
            _errorMessage = "Error al cargar los detalles del crucero. Por favor, inténtalo de nuevo.";
        }
    }

    async Task AddCabinToBooking(CabinSelectionVm cabin)
    {
        if (_isLocking) return;

        // El selector solo ofrece 1..MaxOccupancy, pero un camarote sin pasajeros no es reservable.
        if (cabin.CurrentOccupancy < 1 || cabin.CurrentOccupancy > cabin.MaxOccupancy) return;

        if (GetRemainingCabins(cabin) == 0)
        {
            _snackbar.Add($"No quedan mas camarotes de tipo {cabin.Name}.", Severity.Warning);
            return;
        }

        var occupancy = cabin.CurrentOccupancy;
        var price = Math.Round(cabin.FullPrice / cabin.MaxOccupancy, 2) * occupancy;

        _isLocking = true;
        try
        {
            var result = await _api.LockCabinAsync(cabin.Id, new LockCabinRequest(CruiseDateId, price, occupancy));
            if (!result.IsSuccess)
            {
                _snackbar.Add(result.ErrorMessage!, Severity.Error);
                return;
            }

            SelectedCabins.Add(new CabinSelectionVm
            {
                Id = cabin.Id,
                Name = cabin.Name,
                Description = cabin.Description,
                Number = cabin.Number,
                MaxOccupancy = cabin.MaxOccupancy,
                CurrentOccupancy = occupancy,
                FullPrice = cabin.FullPrice
            });
            _bookingContainer.UpdateCabins(SelectedCabins);

            cabin.CurrentOccupancy = 1;
        }
        finally
        {
            _isLocking = false;
        }
    }

    async Task RemoveCabinFromBooking(CabinSelectionVm cabin)
    {
        if (_isLocking) return;

        _isLocking = true;
        try
        {
            var result = await _api.UnlockCabinAsync(cabin.Id, CruiseDateId);
            if (!result.IsSuccess)
            {
                _snackbar.Add(result.ErrorMessage!, Severity.Error);
                return;
            }

            SelectedCabins.Remove(cabin);
            _bookingContainer.UpdateCabins(SelectedCabins);
        }
        finally
        {
            _isLocking = false;
        }
    }

    int GetRemainingCabins(CabinSelectionVm cabin)
        => Math.Max(cabin.Number - SelectedCabins.Count(selectedCabin => selectedCabin.Id == cabin.Id), 0);

    void AddExtraToBooking(ExtraSelectionVm extra)
    {
        if (!SelectedExtras.Contains(extra))
            SelectedExtras.Add(extra);
    }

    void RemoveExtraFromBooking(ExtraSelectionVm extra) => SelectedExtras.Remove(extra);

    void SaveBooking()
    {
        if (_isLocking) return;

        _bookingContainer.SaveBooking(Cruise, SelectedExtras);
        _navigationManager.NavigateTo("/booking-summary");
    }

    async Task RetryLoadAsync()
    {
        await LoadAsync();
        StateHasChanged();
    }
}

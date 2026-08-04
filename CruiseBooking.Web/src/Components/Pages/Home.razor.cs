using CruiseBooking.Data.GraphQL;
using CruiseBooking.GraphQL.Models;
using CruiseBooking.Mappings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Muestra cruceros destacados y opciones de filtrado en la página principal.
/// </summary>
public partial class Home(ICruisesClient client, NavigationManager navigationManager)
{
    readonly ICruisesClient _client = client;
    readonly NavigationManager _navigationManager = navigationManager;
    IEnumerable<CruiseDto> _featuredCruises = [];

    DateRange _originDateRange = new(DateTime.Now, DateTime.Now.AddYears(1));
    IEnumerable<string> _originPorts = [];
    IEnumerable<string> _selectedOriginPorts = [];
    int MaxDuration = 365;
    bool _adultsOnly = false;
    bool _featuredOnly = false;

    IEnumerable<ShipDto> _ships = [];

    protected override async Task OnInitializedAsync()
    {
        var originPorts = await _client.GetAvailablePorts.ExecuteAsync();
        if (originPorts.Data is null)
            return;
        _originPorts = originPorts.Data.Ports;

        var shipsResult = await _client.GetShips.ExecuteAsync();
        if (shipsResult.Data is not null)
        {
            _ships = shipsResult.Data.Ships.Select(x => x.ToDto());
        }

        var featuredCruises = await _client.GetFeaturedCruises.ExecuteAsync();
        if (featuredCruises.Data is not null)
            _featuredCruises = featuredCruises.Data.FeaturedCruises.Select(x => x.ToCruiseDto());
    }

    string GetCruiseSearchUrl()
    {
        return $"/cruise-search?startDate={_originDateRange.Start:yyyy-MM-dd}" +
            $"&endDate={_originDateRange.End:yyyy-MM-dd}" +
            $"&originPorts={string.Join(",", _selectedOriginPorts)}" +
            $"&maxDuration={MaxDuration}" +
            $"&adultsOnly={_adultsOnly}" +
            $"&featuredOnly={_featuredOnly}";
    }

    void NavigateToFeaturedCruises()
    {
        _navigationManager.NavigateTo("/cruise-search?featuredOnly=true");
    }
}

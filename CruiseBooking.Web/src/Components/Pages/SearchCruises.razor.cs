using CruiseBooking.Data.GraphQL;
using CruiseBooking.GraphQL.Models;
using CruiseBooking.Mappings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Muestra una lista de cruceros con paginación de cursor con opciones de filtrado y ordenamiento.
/// </summary>
public partial class SearchCruises(ICruisesClient client)
{
    bool _expanded = false;
    readonly ICruisesClient _client = client;

    /// <summary>
    /// Obtiene o establece la fecha de inicio para el filtro de búsqueda de cruceros.
    /// </summary>
    [SupplyParameterFromQuery(Name = "startDate")]
    public DateTime? StartDateParam { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha de finalización para el filtro de búsqueda de cruceros.
    /// </summary>
    [SupplyParameterFromQuery(Name = "endDate")]
    public DateTime? EndDateParam { get; set; }

    /// <summary>
    /// Obtiene o establece la lista separada por comas de puertos de origen para filtrar.
    /// </summary>
    [SupplyParameterFromQuery(Name = "originPorts")]
    public string? OriginPortsParam { get; set; }

    /// <summary>
    /// Obtiene o establece la duración máxima en días para el filtro de búsqueda de cruceros.
    /// </summary>
    [SupplyParameterFromQuery(Name = "maxDuration")]
    public int? MaxDurationParam { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si se debe filtrar para cruceros solo para adultos.
    /// </summary>
    [SupplyParameterFromQuery(Name = "adultsOnly")]
    public bool? AdultsOnlyParam { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si se debe filtrar solo para cruceros destacados.
    /// </summary>
    [SupplyParameterFromQuery(Name = "featuredOnly")]
    public bool? FeaturedOnlyParam { get; set; }

    readonly int _elementsPerPage = 10;
    bool _hasNextPage;
    string? _lastCursor;

    CruiseFilterInput? _lastFilter;

    string _sortField = "durationInDays_desc";
    string _lastSortField = "durationInDays_desc";

    DateRange _originDateRange = new(DateTime.Now, DateTime.Now.AddYears(1));
    IEnumerable<string> _originPorts = [];
    IEnumerable<string> _selectedOriginPorts = [];
    int _maxDuration = 365;
    bool _adultsOnly = false;
    bool _featuredOnly = false;

    List<CruiseDto> Cruises { get; set; } = [];
    Dictionary<int, CruiseDateDto> SelectedStartDates { get; set; } = [];

    /// <summary>
    /// Gets or sets the error message to display if the search fails.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var originPorts = await _client.GetAvailablePorts.ExecuteAsync();
        if (originPorts.Data is null)
            return;

        _originPorts = originPorts.Data.Ports;

        _originDateRange = new DateRange(
            StartDateParam ?? DateTime.Now.Date,
            EndDateParam ?? DateTime.Now.AddYears(1).Date);

        _selectedOriginPorts = OriginPortsParam?.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];

       _maxDuration = MaxDurationParam ?? 365;
       _adultsOnly = AdultsOnlyParam ?? false;
        _featuredOnly = FeaturedOnlyParam ?? false;

        await ApplyFilters();
    }

    void OnExpandCollapseClick()
    {
        _expanded = !_expanded;
    }

    async Task ApplyFilters()
    {
        var filter = new CruiseFilterInput();

        if (_adultsOnly)
            filter.AdultsOnly = new BooleanOperationFilterInput { Eq = true };

        if (_featuredOnly)
            filter.Featured = new BooleanOperationFilterInput { Eq = true };

        if (_selectedOriginPorts.Any())
         {
            filter.OriginPort = new StringOperationFilterInput
            {
                In = [.. _selectedOriginPorts]
            };
        }

        if (_maxDuration > 0)
        {
            filter.DurationInDays = new IntOperationFilterInput { Lte = _maxDuration };
        }

        if (_originDateRange.Start is not null && _originDateRange.End is not null)
        {
            filter.Dates = new ListCruiseDateFilterTypeFilterInput
            {
                Some = new CruiseDateFilterInput
                {
                    StartDate = new DateTimeOperationFilterInput
                    {
                        Gte = _originDateRange.Start.Value,
                        Lte = _originDateRange.End.Value
                    }
                }
            };
        }

        _lastFilter = filter;
        _lastCursor = null;
        Cruises.Clear();
        SelectedStartDates.Clear();
        await LoadNextPage();
    }

    async Task ResetFilters()
    {
        _maxDuration = 365;
        _selectedOriginPorts = [];
        _originDateRange = new DateRange(DateTime.Now, DateTime.Now.AddYears(1));
        _adultsOnly = false;
        _featuredOnly = false;

        await ApplyFilters();
    }

    async Task LoadNextPage()
    {
        if(!_lastSortField.Equals(_sortField))
        {
            _lastCursor = null;
            Cruises.Clear();
        }

        _lastSortField = _sortField;

        var sortInput = _sortField switch
        {
            "durationInDays_asc" => new CruiseSortInput { DurationInDays = SortEnumType.Asc },
            "durationInDays_desc" => new CruiseSortInput { DurationInDays = SortEnumType.Desc },
            _ => new CruiseSortInput { DurationInDays = SortEnumType.Desc }
        };

        var result = await _client.SearchCruises.ExecuteAsync(_lastFilter, [sortInput], _elementsPerPage, _lastCursor);
        if (result.Data?.Cruises?.Nodes is not null)
        {
            var newCruises = result.Data.Cruises.Nodes.Select(x => x.ToCruiseDto());
            Cruises.AddRange(newCruises);
            foreach (var cruise in newCruises.Where(x => x.Dates.Count > 0))
            {
                var defaultDate = cruise.Dates[0];
                if (!SelectedStartDates.ContainsKey(cruise.Id))
                    SelectedStartDates.TryAdd(cruise.Id, defaultDate);
            }

            _hasNextPage = result.Data.Cruises.PageInfo.HasNextPage;
            _lastCursor = result.Data.Cruises.PageInfo.EndCursor;
        }
    }

    void OnStartDateChanged(CruiseDto cruise, int selectedDateId)
    {
        var selected = cruise.Dates.FirstOrDefault(d => d.Id == selectedDateId);
        if (selected is not null)
        {
            SelectedStartDates[cruise.Id] = selected;
        }
    }

    string GetBookingUrl(int cruiseId)
    {
        var cruiseDate = SelectedStartDates.TryGetValue(cruiseId, out CruiseDateDto? value)
            ? value.Id
            : 0;

        return $"/book-cruise?cruiseId={cruiseId}&cruiseDateId={cruiseDate}";
    }
}
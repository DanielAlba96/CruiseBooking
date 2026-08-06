using Shared.Domain.Repositories;
using System.ComponentModel;
using System.Text.Json;

namespace Core.Agents.Tools;

/// <summary>
/// Herramientas de solo lectura sobre el catálogo de cruceros expuestas al modelo de lenguaje.
/// Las dependencias se reciben por constructor para que cada método declare únicamente los parámetros
/// que rellena el modelo; así <c>AIFunctionFactory</c> conserva el nombre del método y los
/// <see cref="DescriptionAttribute"/> de la función y de cada parámetro en el esquema JSON.
/// </summary>
internal sealed class CruiseTools(
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository)
{
    [DisplayName("search_cruises")]
    [Description("Busca cruceros por zona, duración o tipo")]
    public async Task<string> SearchCruises(
    [Description("Zona geográfica")] string? zone,
    [Description("Duración mínima en días")] int? minDays,
    [Description("Duración máxima en días")] int? maxDays,
    [Description("Solo para adultos")] bool adultsOnly)
    {
        var results = await cruiseRepository.SearchCruisesAsync(zone, minDays, maxDays, adultsOnly);
        return JsonSerializer.Serialize(results);
    }

    [DisplayName("get_cruise_dates")]
    [Description("Obtiene las fechas de salida de un crucero específico")]
    public async Task<string> GetCruiseDates(
        [Description("ID del crucero")] int cruiseId)
    {
        var result = await cruiseRepository.GetCruiseDatesAsync(cruiseId);
        return JsonSerializer.Serialize(result);
    }


    [DisplayName("get_available_cabins")]
    [Description("Obtiene cabinas disponibles para una fecha de salida de crucero con precios y disponibilidad")]
    public async Task<string> GetAvailableCabins(
        [Description("ID de la fecha de salida del crucero")] int cruiseDateId)
    {
        var result = await cabinRepository.GetCabinsByCruiseDate(cruiseDateId);
        return JsonSerializer.Serialize(result);
    }

    [DisplayName("get_extras")]
    [Description("Obtiene los extras disponibles para una fecha de salida de crucero con sus precios")]
    public async Task<string> GetExtras(
        [Description("ID de la fecha de salida del crucero")] int cruiseDateId)
    {
        var extras = await extraRepository.GetExtrasAsync(cruiseDateId);
        var result = extras.Select(e => new { extra_id = e.ExtraId, name = e.Extra!.Name, description = e.Extra!.Description, price = e.Price });

        return JsonSerializer.Serialize(result);
    }
}

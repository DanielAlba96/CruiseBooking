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
    [Description("Busca cruceros del catálogo filtrando por zona, duración y tipo. Devuelve para cada crucero su id, nombre, zona y duración. El id resultante es el que espera get_cruise_dates. Todos los filtros son opcionales: omítelos para ver el catálogo completo.")]
    public async Task<string> SearchCruises(
    [Description("Zona geográfica del itinerario (Caribe, Mediterráneo, Norte de Europa...). Es lo que suele indicar el usuario, en lugar del nombre del crucero")] string? zone,
    [Description("Duración mínima del crucero en días")] int? minDays,
    [Description("Duración máxima del crucero en días")] int? maxDays,
    [Description("true para devolver únicamente cruceros solo para adultos")] bool adultsOnly)
    {
        var results = await cruiseRepository.SearchCruisesAsync(zone, minDays, maxDays, adultsOnly);
        return JsonSerializer.Serialize(results);
    }

    [DisplayName("get_cruise_dates")]
    [Description("Obtiene las fechas de salida disponibles de un crucero concreto. Cada salida devuelve su propio id (cruise_date_id), distinto del id del crucero: es el que esperan get_available_cabins, get_extras y start_booking. Las cabinas, los extras y sus precios dependen de la salida elegida, no del crucero.")]
    public async Task<string> GetCruiseDates(
        [Description("ID del crucero obtenido de search_cruises")] int cruiseId)
    {
        var result = await cruiseRepository.GetCruiseDatesAsync(cruiseId);
        return JsonSerializer.Serialize(result);
    }


    [DisplayName("get_available_cabins")]
    [Description("Obtiene las cabinas disponibles en una fecha de salida concreta. Cada cabina devuelve su id, su precio por camarote (independiente del número de pasajeros) y su tipo, con el nombre y el aforo máximo (maxOccupancy) en cabinType. Son los valores que espera add_cabin: no los inventes ni los deduzcas de otra fuente.")]
    public async Task<string> GetAvailableCabins(
        [Description("ID de la fecha de salida del crucero obtenido de get_cruise_dates")] int cruiseDateId)
    {
        var result = await cabinRepository.GetCabinsByCruiseDate(cruiseDateId);
        return JsonSerializer.Serialize(result);
    }

    [DisplayName("get_extras")]
    [Description("Obtiene los extras contratables en una fecha de salida concreta: id, nombre, descripción y precio de cada uno. El extra_id devuelto es el que espera add_extra.")]
    public async Task<string> GetExtras(
        [Description("ID de la fecha de salida del crucero obtenido de get_cruise_dates")] int cruiseDateId)
    {
        var extras = await extraRepository.GetExtrasAsync(cruiseDateId);
        var result = extras.Select(e => new { extra_id = e.ExtraId, name = e.Extra!.Name, description = e.Extra!.Description, price = e.Price });

        return JsonSerializer.Serialize(result);
    }
}

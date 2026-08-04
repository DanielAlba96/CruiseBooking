using Shared.Domain.Repositories;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA para búsqueda de cruceros por zona, duración y tipo de pasajeros.
/// </summary>
internal class SearchCruisesTool : Tool, IAsyncInvokableTool
{
    private readonly ICruiseRepository _cruiseRepository;

    /// <summary>
    /// Inicializa una nueva instancia de la herramienta de búsqueda de cruceros.
    /// </summary>
    /// <param name="cruiseRepository">Repositorio de cruceros para acceder a los datos.</param>
    public SearchCruisesTool(ICruiseRepository cruiseRepository)
    {
        _cruiseRepository = cruiseRepository;

        Function = new Function
        {
            Name = "search_cruises",
            Description = "Busca cruceros por zona, duración o tipo",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["zone"] = new() { Type = "string", Description = "Zona geográfica" },
                    ["min_days"] = new() { Type = "integer", Description = "Duración mínima en días" },
                    ["max_days"] = new() { Type = "integer", Description = "Duración máxima en días" },
                    ["adults_only"] = new() { Type = "boolean", Description = "Solo para adultos" }
                }
            }
        };
    }

    /// <summary>
    /// Invoca la búsqueda de cruceros con los argumentos especificados.
    /// </summary>
    /// <param name="args">Diccionario con los parámetros de búsqueda: 'zone', 'min_days', 'max_days', 'adults_only'.</param>
    /// <returns>Resultado de la búsqueda serializado en JSON, o null si no hay resultados.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var zone = args != null && args.TryGetValue("zone", out var z) ? z as string : null;
        var minDays = args != null && args.TryGetValue("min_days", out var mn) ? (int?)Convert.ToInt32(mn) : null;
        var maxDays = args != null && args.TryGetValue("max_days", out var mx) ? (int?)Convert.ToInt32(mx) : null;
        var adultsOnly = args != null && args.TryGetValue("adults_only", out var ao) && ao is bool b && b;

        var results = await _cruiseRepository.SearchCruisesAsync(zone, minDays, maxDays, adultsOnly);
        return JsonSerializer.Serialize(results);
    }
}

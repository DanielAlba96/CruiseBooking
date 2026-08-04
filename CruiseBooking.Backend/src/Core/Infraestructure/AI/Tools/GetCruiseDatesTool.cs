using Shared.Domain.Repositories;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta que obtiene las fechas de salida de un crucero específico para el asistente de IA.
/// </summary>
internal class GetCruiseDatesTool : Tool, IAsyncInvokableTool
{
    private readonly ICruiseRepository _cruiseRepository;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GetCruiseDatesTool"/>.
    /// </summary>
    /// <param name="cruiseRepository">Repositorio de cruceros para acceder a los datos.</param>
    public GetCruiseDatesTool(ICruiseRepository cruiseRepository)
    {
        _cruiseRepository = cruiseRepository;

        Function = new Function
        {
            Name = "get_cruise_dates",
            Description = "Obtiene las fechas de salida de un crucero específico",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["cruise_id"] = new() { Type = "integer", Description = "ID del crucero" }
                },
                Required = ["cruise_id"]
            }
        };
    }

    /// <summary>
    /// Invoca la herramienta para obtener las fechas de un crucero de forma asincrónica.
    /// </summary>
    /// <param name="args">Diccionario de argumentos que contiene el parámetro "cruise_id" con el ID del crucero.</param>
    /// <returns>Serialización JSON de las fechas de salida del crucero solicitado.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var cruiseId = args != null && args.TryGetValue("cruise_id", out var cid) ? Convert.ToInt32(cid) : 0;
        var result = await _cruiseRepository.GetCruiseDatesAsync(cruiseId);
        return JsonSerializer.Serialize(result);
    }
}

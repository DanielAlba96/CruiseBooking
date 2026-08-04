using Shared.Domain.Repositories;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA que obtiene cabinas disponibles para una fecha de salida de crucero.
/// </summary>
internal class GetAvailableCabinsTool : Tool, IAsyncInvokableTool
{
    private readonly ICabinRepository _cabinRepository;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GetAvailableCabinsTool"/>.
    /// </summary>
    /// <param name="cabinRepository">Repositorio de cabinas para acceder a los datos.</param>
    public GetAvailableCabinsTool(ICabinRepository cabinRepository)
    {
        _cabinRepository = cabinRepository;

        Function = new Function
        {
            Name = "get_available_cabins",
            Description = "Obtiene cabinas disponibles para una fecha de salida de crucero con precios y disponibilidad",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["cruise_date_id"] = new() { Type = "integer", Description = "ID de la fecha de salida del crucero" }
                },
                Required = ["cruise_date_id"]
            }
        };
    }

    /// <summary>
    /// Invoca la herramienta para obtener cabinas disponibles según los parámetros proporcionados.
    /// </summary>
    /// <param name="args">Diccionario de argumentos que contiene el identificador de la fecha del crucero.</param>
    /// <returns>Un objeto serializado en JSON con las cabinas disponibles o nulo si no se encuentran.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var cruiseDateId = args != null && args.TryGetValue("cruise_date_id", out var cdid) ? Convert.ToInt32(cdid) : 0;
        var result = await _cabinRepository.GetCabinsByCruiseDate(cruiseDateId);
        return JsonSerializer.Serialize(result);
    }
}

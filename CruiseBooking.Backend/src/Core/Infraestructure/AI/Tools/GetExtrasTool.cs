using Shared.Domain.Repositories;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA para obtener los extras disponibles de una fecha de salida de crucero.
/// </summary>
internal class GetExtrasTool : Tool, IAsyncInvokableTool
{
    private readonly IExtraRepository _extraRepository;

    /// <summary>
    /// Inicializa una nueva instancia de la herramienta GetExtrasTool.
    /// </summary>
    /// <param name="extraRepository">Repositorio de extras para acceder a los datos.</param>
    public GetExtrasTool(IExtraRepository extraRepository)
    {
        _extraRepository = extraRepository;

        Function = new Function
        {
            Name = "get_extras",
            Description = "Obtiene los extras disponibles para una fecha de salida de crucero con sus precios",
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
    /// Obtiene los extras disponibles para una fecha de salida de crucero especificada.
    /// </summary>
    /// <param name="args">Diccionario de argumentos que contiene el identificador de la fecha del crucero bajo la clave "cruise_date_id".</param>
    /// <returns>Cadena JSON serializada que contiene una colección de extras con sus identificadores, nombres, descripciones y precios.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var cruiseDateId = args != null && args.TryGetValue("cruise_date_id", out var cdid) ? Convert.ToInt32(cdid) : 0;

        var extras = await _extraRepository.GetExtrasAsync(cruiseDateId);
        var result = extras.Select(e => new { extra_id = e.ExtraId, name = e.Extra!.Name, description = e.Extra!.Description, price = e.Price });

        return JsonSerializer.Serialize(result);
    }
}

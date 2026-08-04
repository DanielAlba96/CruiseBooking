using Shared.Domain.Repositories;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta para añadir extras a un borrador de reserva en sesiones de IA.
/// </summary>
internal class AddExtraTool : Tool, IAsyncInvokableTool
{
    private readonly IExtraRepository _extraRepository;
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AddExtraTool"/>.
    /// </summary>
    /// <param name="extraRepository">Repositorio de extras del crucero.</param>
    /// <param name="store">Almacén de borradores de reserva.</param>
    /// <param name="sessionId">Identificador único de la sesión de IA.</param>
    public AddExtraTool(IExtraRepository extraRepository, BookingDraftStore store, Guid sessionId)
    {
        _extraRepository = extraRepository;
        _store = store;
        _sessionId = sessionId;

        Function = new Function
        {
            Name = "add_extra",
            Description = "Añade un extra al borrador de reserva. Requiere haber iniciado la reserva con start_booking.",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["extra_id"] = new() { Type = "integer", Description = "ID del extra" }
                },
                Required = ["extra_id"]
            }
        };
    }

    /// <summary>
    /// Ejecuta la acción de añadir un extra al borrador de reserva.
    /// </summary>
    /// <param name="args">Diccionario de argumentos que contiene "extra_id" con el identificador del extra.</param>
    /// <returns>Objeto JSON serealizado con el resultado de la operación o un error si falló.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var extraId = args != null && args.TryGetValue("extra_id", out var eid) ? Convert.ToInt32(eid) : 0;

        var draft = _store.Get(_sessionId);
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "Primero inicia la reserva con start_booking" });
        }

        if (draft.Extras.Any(e => e.ExtraId == extraId))
        {
            return JsonSerializer.Serialize(new { error = "El extra ya está en la reserva" });
        }

        var extra = await _extraRepository.GetCruiseDateExtraAsync(draft.CruiseDateId, extraId);
        if (extra is null)
        {
            return JsonSerializer.Serialize(new { error = "El extra no está disponible para este crucero" });
        }

        draft.Extras.Add(new DraftExtra
        {
            ExtraId = extra.ExtraId,
            Name = extra.Extra!.Name,
            Price = extra.Price
        });

        return JsonSerializer.Serialize(new { ok = true, extra_id = extra.ExtraId, name = extra.Extra!.Name, price = extra.Price });
    }
}

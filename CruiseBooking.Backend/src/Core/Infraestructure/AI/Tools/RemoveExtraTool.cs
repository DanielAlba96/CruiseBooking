using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA que permite eliminar un extra del borrador de reserva.
/// </summary>
internal class RemoveExtraTool : Tool, IAsyncInvokableTool
{
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RemoveExtraTool"/>.
    /// </summary>
    /// <param name="store">Almacén de borradores de reserva.</param>
    /// <param name="sessionId">Identificador de la sesión del usuario.</param>
    public RemoveExtraTool(BookingDraftStore store, Guid sessionId)
    {
        _store = store;
        _sessionId = sessionId;

        Function = new Function
        {
            Name = "remove_extra",
            Description = "Quita un extra del borrador de reserva.",
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
    /// Invoca la herramienta para eliminar un extra del borrador de reserva.
    /// </summary>
    /// <param name="args">Diccionario con los argumentos, incluyendo "extra_id".</param>
    /// <returns>Tarea con resultado JSON indicando éxito o error.</returns>
    public Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var extraId = args != null && args.TryGetValue("extra_id", out var eid) ? Convert.ToInt32(eid) : 0;

        var draft = _store.Get(_sessionId);
        var extra = draft?.Extras.FirstOrDefault(e => e.ExtraId == extraId);
        if (extra is null)
        {
            return Task.FromResult<object?>(JsonSerializer.Serialize(new { error = "El extra no está en la reserva" }));
        }

        draft!.Extras.Remove(extra);

        return Task.FromResult<object?>(JsonSerializer.Serialize(new { ok = true, extra_id = extraId }));
    }
}

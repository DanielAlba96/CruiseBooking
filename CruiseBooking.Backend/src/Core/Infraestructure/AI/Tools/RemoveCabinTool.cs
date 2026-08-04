using Core.Application.Models;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using Shared.Domain.Repositories;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA que permite quitar una cabina del borrador de reserva.
/// </summary>
internal class RemoveCabinTool : Tool, IAsyncInvokableTool
{
    private readonly ICabinRepository _cabinRepository;
    private readonly UserInfo _userInfo;
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="RemoveCabinTool"/>.
    /// </summary>
    /// <param name="cabinRepository">Repositorio para acceder y modificar datos de cabinas.</param>
    /// <param name="userInfo">Información del usuario actual.</param>
    /// <param name="store">Almacén de borradores de reserva.</param>
    /// <param name="sessionId">ID de la sesión de reserva.</param>
    public RemoveCabinTool(ICabinRepository cabinRepository, UserInfo userInfo, BookingDraftStore store, Guid sessionId)
    {
        _cabinRepository = cabinRepository;
        _userInfo = userInfo;
        _store = store;
        _sessionId = sessionId;

        Function = new Function
        {
            Name = "remove_cabin",
            Description = "Quita una cabina del borrador de reserva.",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["cabin_id"] = new() { Type = "integer", Description = "ID de la cabina del barco" }
                },
                Required = ["cabin_id"]
            }
        };
    }

    /// <summary>
    /// Invoca la herramienta para quitar una cabina del borrador de reserva.
    /// </summary>
    /// <param name="args">Diccionario de argumentos que contiene el "cabin_id" de la cabina a quitar.</param>
    /// <returns>Objeto serializado en JSON con el resultado de la operación: éxito o error.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var cabinId = args != null && args.TryGetValue("cabin_id", out var cid) ? Convert.ToInt32(cid) : 0;

        var draft = _store.Get(_sessionId);
        var matches = draft?.Cabins.Where(c => c.CabinId == cabinId).ToList() ?? [];
        if (matches.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "La cabina no está en la reserva" });
        }

        await _cabinRepository.UnlockCabin(draft!.CruiseDateId, cabinId, _userInfo.Id);
        draft.Cabins.Remove(matches[^1]);

        var remaining = draft.Cabins.Count(c => c.CabinId == cabinId);

        return JsonSerializer.Serialize(new { ok = true, cabin_id = cabinId, remaining });
    }
}

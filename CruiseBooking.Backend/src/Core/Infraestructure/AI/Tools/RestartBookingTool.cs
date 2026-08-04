using Core.Application.Models;
using Shared.Domain.Repositories;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta para reiniciar una reserva desde cero, liberando cabinas bloqueadas y descartando el borrador.
/// </summary>
internal class RestartBookingTool : Tool, IAsyncInvokableTool
{
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;
    private readonly ICabinRepository _cabinRepository;
    private readonly UserInfo _userInfo;

    /// <summary>
    /// Inicializa una nueva instancia de la herramienta RestartBookingTool.
    /// </summary>
    /// <param name="store">Almacén de borradores de reserva.</param>
    /// <param name="sessionId">Identificador de la sesión.</param>
    /// <param name="cabinRepository">Repositorio de cabinas.</param>
    /// <param name="userInfo">Información del usuario actual.</param>
    public RestartBookingTool(BookingDraftStore store, Guid sessionId, ICabinRepository cabinRepository, UserInfo userInfo)
    {
        _store = store;
        _sessionId = sessionId;
        _cabinRepository = cabinRepository;
        _userInfo = userInfo;

        Function = new Function
        {
            Name = "restart_booking",
            Description = "Reinicia la reserva desde cero: libera las cabinas bloqueadas y descarta el borrador completo. Llamar SOLO tras la confirmación explícita del usuario.",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>()
            }
        };
    }

    /// <summary>
    /// Invoca la operación de reinicio de reserva de forma asincrónica.
    /// </summary>
    /// <param name="args">Argumentos de la invocación (no utilizado en esta herramienta).</param>
    /// <returns>Un JSON serializado con el resultado de la operación: éxito o error.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var draft = _store.Get(_sessionId);
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "No hay ninguna reserva iniciada" });
        }

        await _cabinRepository.ClearLockedCabins(draft.CruiseDateId, _userInfo.Id);
        _store.Clear(_sessionId);

        return JsonSerializer.Serialize(new { ok = true, message = "Reserva reiniciada. Empieza de nuevo con start_booking." });
    }
}

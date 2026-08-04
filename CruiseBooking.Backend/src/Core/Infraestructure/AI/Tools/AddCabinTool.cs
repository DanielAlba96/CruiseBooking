using Core.Application.Models;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using Shared.Domain.Exceptions;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA que permite añadir cabinas a un borrador de reserva.
/// Requiere que se haya iniciado la reserva previamente con la herramienta start_booking.
/// </summary>
internal class AddCabinTool : Tool, IAsyncInvokableTool
{
    private readonly ICabinRepository _cabinRepository;
    private readonly IJobService _jobService;
    private readonly UserInfo _userInfo;
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;

    /// <summary>
    /// Inicializa una nueva instancia de la herramienta para añadir cabinas.
    /// </summary>
    /// <param name="cabinRepository">Repositorio de cabinas para consultar y bloquear disponibilidad.</param>
    /// <param name="jobService">Servicio para programar trabajos de limpieza de bloqueos.</param>
    /// <param name="userInfo">Información del usuario autenticado que realiza la reserva.</param>
    /// <param name="store">Almacén de borradores de reserva en sesión.</param>
    /// <param name="sessionId">Identificador único de la sesión de reserva.</param>
    public AddCabinTool(ICabinRepository cabinRepository, IJobService jobService, UserInfo userInfo, BookingDraftStore store, Guid sessionId)
    {
        _cabinRepository = cabinRepository;
        _jobService = jobService;
        _userInfo = userInfo;
        _store = store;
        _sessionId = sessionId;

        Function = new Function
        {
            Name = "add_cabin",
            Description = "Añade una o varias cabinas del mismo tipo al borrador de reserva. Requiere haber iniciado la reserva con start_booking.",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>
                {
                    ["cabin_id"] = new() { Type = "integer", Description = "ID de la cabina del barco" },
                    ["price"] = new() { Type = "number", Description = "Precio de la cabina obtenido de get_available_cabins" },
                    ["max_occupancy"] = new() { Type = "integer", Description = "Aforo máximo de la cabina obtenido de get_available_cabins" },
                    ["occupants"] = new() { Type = "integer", Description = "Número de pasajeros que ocuparán cada cabina de este tipo (no puede superar max_occupancy)" },
                    ["quantity"] = new() { Type = "integer", Description = "Número de cabinas de este tipo a añadir (por defecto 1)" }
                },
                Required = ["cabin_id", "price", "max_occupancy", "occupants"]
            }
        };
    }

    /// <summary>
    /// Invoca la herramienta para añadir una o varias cabinas del mismo tipo al borrador de reserva.
    /// </summary>
    /// <param name="args">Diccionario con los parámetros de la cabina:
    /// - cabin_id (int): Identificador de la cabina.
    /// - price (decimal): Precio de la cabina.
    /// - max_occupancy (int): Aforo máximo de la cabina.
    /// - occupants (int): Número de pasajeros por cabina.
    /// - quantity (int): Número de cabinas a añadir (por defecto 1).</param>
    /// <returns>Objeto JSON con el resultado de la operación.
    /// En caso de éxito: { ok: true, cabin_id, price, max_occupancy, quantity, total_of_type }.
    /// En caso de error: { error: mensaje, locked: cantidad bloqueada }.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var cabinId = args != null && args.TryGetValue("cabin_id", out var cid) ? Convert.ToInt32(cid) : 0;
        var price = args != null && args.TryGetValue("price", out var p) ? Convert.ToDecimal(p) : 0m;
        var maxOccupancy = args != null && args.TryGetValue("max_occupancy", out var mo) ? Convert.ToInt32(mo) : 0;
        var occupants = args != null && args.TryGetValue("occupants", out var o) ? Math.Max(1, Convert.ToInt32(o)) : 1;
        var quantity = args != null && args.TryGetValue("quantity", out var q) ? Math.Max(1, Convert.ToInt32(q)) : 1;

        var draft = _store.Get(_sessionId);
        if (draft is null)
        {
            return JsonSerializer.Serialize(new { error = "Primero inicia la reserva con start_booking" });
        }

        for (int i = 0; i < quantity; i++)
        {
            try
            {
                var lockedCabinId = await _cabinRepository.LockCabin(draft.CruiseDateId, _userInfo.Id, cabinId, price, occupants);
                await _jobService.ScheduleLockedCabinCleanUp([lockedCabinId]);
            }
            catch (ControlledException ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, locked = draft.Cabins.Count(c => c.CabinId == cabinId) });
            }

            draft.Cabins.Add(new DraftCabin
            {
                CabinId = cabinId,
                Price = price,
                MaxOccupancy = maxOccupancy,
                Occupants = occupants
            });
        }

        var totalOfType = draft.Cabins.Count(c => c.CabinId == cabinId);

        return JsonSerializer.Serialize(new { ok = true, cabin_id = cabinId, price, max_occupancy = maxOccupancy, quantity, total_of_type = totalOfType });
    }
}

using System.Text.Json;
using Core.Application.Models;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using Shared.Domain.Repositories;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta que proporciona un resumen del borrador de reserva con cabinas, ocupantes, extras y precio total.
/// </summary>
internal class GetBookingSummaryTool : Tool, IAsyncInvokableTool
{
    private readonly ICabinRepository _cabinRepository;
    private readonly IExtraRepository _extraRepository;
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GetBookingSummaryTool"/>.
    /// </summary>
    /// <param name="cabinRepository">Repositorio para acceder a los datos de cabinas.</param>
    /// <param name="extraRepository">Repositorio para acceder a los datos de extras.</param>
    /// <param name="store">Almacén del borrador de reserva.</param>
    /// <param name="sessionId">Identificador de la sesión del usuario.</param>
    public GetBookingSummaryTool(
        ICabinRepository cabinRepository,
        IExtraRepository extraRepository,
        BookingDraftStore store,
        Guid sessionId)
    {
        _cabinRepository = cabinRepository;
        _extraRepository = extraRepository;
        _store = store;
        _sessionId = sessionId;

        Function = new Function
        {
            Name = "get_booking_summary",
            Description = "Devuelve el resumen del borrador de reserva con cabinas, número de ocupantes, extras y el precio total, y bloquea las cabinas con los precios mostrados.",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>()
            }
        };
    }

    /// <summary>
    /// Obtiene el resumen del borrador de reserva actual, incluyendo cabinas, extras y precio total.
    /// Bloquea los precios de las cabinas al momento de invocar la herramienta.
    /// </summary>
    /// <param name="args">Argumentos de invocación (no utilizados en esta herramienta).</param>
    /// <returns>
    /// Un objeto JSON serializado que contiene el resumen de la reserva con cabinas, extras y precio total,
    /// o un mensaje de error si el borrador está vacío o contiene elementos inválidos.
    /// </returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var draft = _store.Get(_sessionId);
        if (draft is null || (draft.Cabins.Count == 0 && draft.Extras.Count == 0))
        {
            return JsonSerializer.Serialize(new { error = "El borrador de reserva está vacío" });
        }

        var availableCabins = await _cabinRepository.GetCabinsByCruiseDate(draft.CruiseDateId);
        var cabinPrices = availableCabins.ToDictionary(c => c.Id, c => c.Price);
        var availableExtras = await _extraRepository.GetExtrasAsync(draft.CruiseDateId);
        var extraPrices = availableExtras.ToDictionary(e => e.ExtraId, e => e.Price);

        if (draft.Cabins.Any(c => !cabinPrices.ContainsKey(c.CabinId)))
        {
            return JsonSerializer.Serialize(new { error = "Alguna de las cabinas del borrador ya no existe en esta fecha de salida. Pide al usuario que la elimine con remove_cabin o que reinicie la reserva con restart_booking." });
        }

        if (draft.Extras.Any(e => !extraPrices.ContainsKey(e.ExtraId)))
        {
            return JsonSerializer.Serialize(new { error = "Alguno de los extras del borrador ya no existe en esta fecha de salida. Pide al usuario que lo elimine con remove_extra o que reinicie la reserva con restart_booking." });
        }

        var cabins = draft.Cabins.GroupBy(c => c.CabinId).Select(g =>
        {
            var unitPrice = cabinPrices[g.Key];
            return new
            {
                cabinId = g.Key,
                quantity = g.Count(),
                unitPrice,
                lineTotal = unitPrice * g.Count(),
                maxOccupancy = g.First().MaxOccupancy
            };
        }).ToList();

        var extras = draft.Extras.Select(e => new { extraId = e.ExtraId, e.Name, price = extraPrices[e.ExtraId] }).ToList();

        var total = cabins.Sum(c => c.lineTotal) + extras.Sum(e => e.price);

        return JsonSerializer.Serialize(new { cabins, extras, total });
    }
}

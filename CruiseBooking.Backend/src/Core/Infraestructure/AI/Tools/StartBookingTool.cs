using Shared.Domain.Repositories;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using System.Text.Json;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de IA para iniciar un borrador de reserva de crucero basada en una fecha de salida específica.
/// </summary>
internal class StartBookingTool : Tool, IAsyncInvokableTool
{
    private readonly ICruiseRepository _cruiseRepository;
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="StartBookingTool"/>.
    /// </summary>
    /// <param name="cruiseRepository">Repositorio para acceder a información de cruceros.</param>
    /// <param name="store">Almacén para gestionar borradores de reservas.</param>
    /// <param name="sessionId">Identificador único de la sesión de IA.</param>
    public StartBookingTool(ICruiseRepository cruiseRepository, BookingDraftStore store, Guid sessionId)
    {
        _cruiseRepository = cruiseRepository;
        _store = store;
        _sessionId = sessionId;

        Function = new Function
        {
            Name = "start_booking",
            Description = "Inicia un borrador de reserva para una fecha de salida de crucero. Reinicia cualquier borrador anterior.",
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
    /// Invoca la herramienta para iniciar un nuevo borrador de reserva en la sesión actual.
    /// </summary>
    /// <param name="args">Argumentos de la herramienta con la clave "cruise_date_id" que contiene el identificador de la fecha de salida del crucero.</param>
    /// <returns>
    /// Una cadena JSON serializada con un objeto que contiene:
    /// - Si tiene éxito: <c>{ "ok": true, "cruise_date_id": &lt;id&gt; }</c>
    /// - Si falla: <c>{ "error": "&lt;mensaje de error&gt;" }</c>
    /// </returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var cruiseDateId = args != null && args.TryGetValue("cruise_date_id", out var cdid) ? Convert.ToInt32(cdid) : 0;

        var cruiseDate = await _cruiseRepository.GetCruiseDateAsync(cruiseDateId);
        if (cruiseDate is null)
        {
            return JsonSerializer.Serialize(new { error = "La fecha de salida de crucero no existe" });
        }

        _store.Set(_sessionId, new BookingDraft { CruiseDateId = cruiseDate.Id, ShipId = cruiseDate.ShipId });

        return JsonSerializer.Serialize(new { ok = true, cruise_date_id = cruiseDate.Id });
    }
}

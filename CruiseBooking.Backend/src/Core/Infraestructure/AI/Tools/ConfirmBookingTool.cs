using System.Text.Json;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;
using Core.Infrastructure.AI.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using Shared.Domain.Exceptions;

namespace Core.Infrastructure.AI.Tools;

/// <summary>
/// Herramienta de confirmación de reserva que procesa la confirmación de una reserva de crucero mediante el mediador.
/// </summary>
internal class ConfirmBookingTool : Tool, IAsyncInvokableTool
{
    private readonly BookingDraftStore _store;
    private readonly Guid _sessionId;
    private readonly IMediator _mediator;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ConfirmBookingTool"/>.
    /// </summary>
    /// <param name="store">Almacén de borradores de reservas.</param>
    /// <param name="sessionId">Identificador único de la sesión de usuario.</param>
    /// <param name="mediator">Mediador para enviar comandos de reserva.</param>
    public ConfirmBookingTool(
        BookingDraftStore store,
        Guid sessionId,
        IMediator mediator)
    {
        _store = store;
        _sessionId = sessionId;
        _mediator = mediator;

        Function = new Function
        {
            Name = "confirm_booking",
            Description = "Confirma la reserva: cobra con el método de pago del usuario y la guarda. Llamar solo tras la confirmación explícita del usuario sobre el resumen.",
            Parameters = new Parameters
            {
                Properties = new Dictionary<string, Property>()
            }
        };
    }

    /// <summary>
    /// Invoca la confirmación de la reserva: cobra con el método de pago del usuario y la guarda.
    /// </summary>
    /// <param name="args">Argumentos del método (no se utilizan en esta implementación).</param>
    /// <returns>Objeto JSON serializado con el resultado de la operación. Contiene "error" en caso de fallo o "ok" con mensaje de confirmación si es exitoso.</returns>
    public async Task<object?> InvokeMethodAsync(IDictionary<string, object?>? args)
    {
        var draft = _store.Get(_sessionId);
        if (draft is null)
            return JsonSerializer.Serialize(new { error = "No hay cabinas en la reserva" });

        var bookingDto = new CreateBookingRequest
        {
            CruiseDateId = draft.CruiseDateId,
            SelectedCabins = [.. draft.Cabins.Select(c => new BookingCabinSelection(c.CabinId, c.Occupants, c.Price))],
            SelectedExtras = [.. draft.Extras.Select(e => e.ExtraId)]
        };

        try
        {
            await _mediator.Send(new CompleteBooking(bookingDto));
        }
        catch (ControlledException ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }

        _store.Clear(_sessionId);

        return JsonSerializer.Serialize(new
        {
            ok = true,
            check_in = "El usuario recibirá un email con la factura y un enlace para completar el check-in online con los datos de los pasajeros antes de la salida."
        });
    }
}

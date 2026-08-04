using Carter;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Bookings;

namespace Core.Api.Endpoints;

/// <summary>Módulo Carter que expone endpoints para crear, listar, obtener detalle y cancelar reservas.</summary>
public sealed class BookingModule : ICarterModule
{
    /// <summary>Registra los endpoints de reservas en el constructor de rutas.</summary>
    /// <param name="app">Generador de rutas de endpoints.</param>
    /// <remarks>
    /// Configura cinco endpoints bajo el grupo <c>/api/booking</c>:
    /// <list type="bullet">
    /// <item><description>POST <c></c> — Crear una nueva reserva.</description></item>
    /// <item><description>GET <c>/list</c> — Obtener un resumen de todas las reservas del usuario.</description></item>
    /// <item><description>GET <c>/{bookingId:int}</c> — Obtener detalles completos de una reserva.</description></item>
    /// <item><description>DELETE <c>/{bookingId:int}</c> — Cancelar una reserva existente.</description></item>
    /// <item><description>POST <c>/{bookingId:int}/pay</c> — Procesar el pago de una reserva.</description></item>
    /// </list>
    /// Todos los endpoints requieren autorización válida.
    /// </remarks>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var bookingsGroup = app.MapGroup("/api/booking").RequireAuthorization();

        bookingsGroup.MapPost("", async (
            CreateBookingRequest bookingDto,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new CompleteBooking(bookingDto), cancellationToken);
            return Results.NoContent();
        });

        bookingsGroup.MapGet("/list", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var list = await mediator.Send(new GetBookingsSummary(), cancellationToken);
            return Results.Ok(list);
        });

        bookingsGroup.MapGet("/{bookingId:int}", async (
            int bookingId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var detail = await mediator.Send(new GetBookingDetail(bookingId), cancellationToken);
            return Results.Ok(detail);
        });

        bookingsGroup.MapDelete("/{bookingId:int}", async (
            int bookingId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new CancelBooking(bookingId), cancellationToken);
            return Results.NoContent();
        });

        bookingsGroup.MapPost("/{bookingId:int}/pay", async (
            int bookingId,
            PayBookingRequest payment,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new PayBooking(bookingId, payment), cancellationToken);
            return Results.NoContent();
        });
    }
}

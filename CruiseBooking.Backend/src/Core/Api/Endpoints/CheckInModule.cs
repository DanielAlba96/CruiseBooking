using Carter;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn;

namespace Core.Api.Endpoints;

/// <summary>
/// Carter module exposing the online check-in endpoints via two parallel authorization paths:
/// the anonymous opaque check-in token sent by email, and an authenticated JWT for the booking's
/// owner accessing it directly from the frontend.
/// Passengers are managed incrementally (add/remove) until the check-in is explicitly completed.
/// </summary>
public sealed class CheckInModule : ICarterModule
{
    /// <summary>
    /// Configura los endpoints de check-in online incluyendo dos rutas de autorización:
    /// anónima (con token opaco enviado por correo) y autenticada (JWT del propietario de la reserva).
    /// </summary>
    /// <param name="app">El constructor de rutas para registrar los endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var checkInGroup = app.MapGroup("/api/check-in").AllowAnonymous();

        checkInGroup.MapGet("/{token}", async (
            string token,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var checkIn = await mediator.Send(new GetCheckInByToken(token), cancellationToken);
            return Results.Ok(checkIn);
        });

        checkInGroup.MapPost("/{token}/cabins/{bookingCabinId:int}/passengers", async (
            string token,
            int bookingCabinId,
            BookingCabinOccupantRequest passenger,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var passengerId = await mediator.Send(
                new AddCheckInPassengerByToken(token, bookingCabinId, passenger), cancellationToken);

            return Results.Created($"/api/check-in/{token}/passengers/{passengerId}", new { passengerId });
        });

        checkInGroup.MapDelete("/{token}/passengers/{passengerId:int}", async (
            string token,
            int passengerId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveCheckInPassengerByToken(token, passengerId), cancellationToken);
            return Results.NoContent();
        });

        checkInGroup.MapPost("/{token}/complete", async (
            string token,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new CompleteCheckInByToken(token), cancellationToken);
            return Results.NoContent();
        });

        var authCheckInGroup = app.MapGroup("/api/check-in/booking").RequireAuthorization();

        authCheckInGroup.MapGet("/{bookingId:int}", async (
            int bookingId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var checkIn = await mediator.Send(new GetCheckInByBooking(bookingId), cancellationToken);
            return Results.Ok(checkIn);
        });

        authCheckInGroup.MapPost("/{bookingId:int}/cabins/{bookingCabinId:int}/passengers", async (
            int bookingId,
            int bookingCabinId,
            BookingCabinOccupantRequest passenger,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var passengerId = await mediator.Send(
                new AddCheckInPassengerByBooking(bookingId, bookingCabinId, passenger), cancellationToken);

            return Results.Created($"/api/check-in/booking/{bookingId}/passengers/{passengerId}", new { passengerId });
        });

        authCheckInGroup.MapDelete("/{bookingId:int}/passengers/{passengerId:int}", async (
            int bookingId,
            int passengerId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveCheckInPassengerByBooking(bookingId, passengerId), cancellationToken);
            return Results.NoContent();
        });

        authCheckInGroup.MapPost("/{bookingId:int}/complete", async (
            int bookingId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new CompleteCheckInByBooking(bookingId), cancellationToken);
            return Results.NoContent();
        });
    }
}

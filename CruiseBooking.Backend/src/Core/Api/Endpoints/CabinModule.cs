using Carter;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Cabins;

namespace Core.Api.Endpoints;

/// <summary>
/// Módulo Carter que expone los endpoints de bloqueo y gestión de cabinas.
/// Proporciona operaciones para bloquear cabinas, desbloquearlas y consultar el estado de bloqueo.
/// </summary>
public sealed class CabinModule : ICarterModule
{
    /// <summary>
    /// Registra las rutas de API para las operaciones de gestión de cabinas.
    /// Configura tres endpoints:
    /// - PUT /api/cabin/{cabinId}/lock: Bloquea una cabina específica.
    /// - DELETE /api/cabin/{cabinId}/lock: Desbloquea una cabina específica.
    /// - GET /api/cabin/lock/list: Obtiene la lista de cabinas bloqueadas.
    /// </summary>
    /// <param name="app">Constructor de rutas de endpoint para registrar las nuevas rutas.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cabin").RequireAuthorization();

        group.MapPut("/{cabinId:int}/lock", async (
            int cabinId,
            LockCabinRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var lockedCabinId = await mediator.Send(new LockCabin(request.CruiseDateId, cabinId, request.Occupants), cancellationToken);
            return Results.Ok(lockedCabinId);
        });

        group.MapDelete("/{cabinId:int}/lock", async (
            int cruiseDateId,
            int cabinId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new UnlockCabin(cruiseDateId, cabinId), cancellationToken);
            return Results.NoContent();
        });

        group.MapGet("/lock/list", async (
            int cruiseDateId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var lockedCabins = await mediator.Send(new GetLockedCabins(cruiseDateId), cancellationToken);
            return Results.Ok(lockedCabins);
        });
    }
}

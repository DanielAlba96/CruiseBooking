using Carter;
using Core.Application.Common.CQRS;
using Core.Application.UseCases.CruiseDates;

namespace Core.Api.Endpoints;

/// <summary>Módulo de Carter que expone los endpoints de consulta de fechas de crucero.</summary>
public sealed class CruiseDateModule : ICarterModule
{
    /// <summary>Añade las rutas de consulta de fechas de crucero al constructor de rutas.</summary>
    /// <param name="app">Constructor de rutas de ASP.NET Core.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cruise-date/");

        group.MapGet("{cruiseDateId:int}", async (
            int cruiseDateId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var response = await mediator.Send(new GetCruiseDate(cruiseDateId), cancellationToken);
            return Results.Ok(response);
        });
    }
}

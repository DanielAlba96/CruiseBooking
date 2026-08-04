using Carter;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.Users;

namespace Core.Api.Endpoints;

/// <summary>
/// Módulo Carter que expone los endpoints del perfil del usuario actual.
/// </summary>
public sealed class UserModule : ICarterModule
{
    /// <summary>
    /// Registra las rutas para gestionar el perfil del usuario actual.
    /// </summary>
    /// <param name="app">Constructor de rutas de endpoints para registrar las rutas.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user/current").RequireAuthorization();

        group.MapPost("", async (
            CreateOrUpdateUserRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new CreateOrUpdateUser(request), cancellationToken);
            return Results.NoContent();
        });
    }
}

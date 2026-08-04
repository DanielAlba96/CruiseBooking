using Carter;
using Core.Application.Common.CQRS;
using Core.Application.UseCases.Payments;

namespace Core.Api.Endpoints;

/// <summary>Módulo de Carter que expone los puntos finales de gestión de métodos de pago y cálculo de impuestos para el usuario actual.</summary>
public sealed class PaymentModule : ICarterModule
{
    /// <summary>Agrega las rutas de pago al constructor de rutas de la aplicación.</summary>
    /// <param name="app">El constructor de rutas de la aplicación donde se registrarán los puntos finales.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var paymentGroup = app.MapGroup("/api/payment").RequireAuthorization();

        paymentGroup.MapPost("method", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new AddPaymentMethod(), cancellationToken);
            return Results.Ok(result);
        });

        paymentGroup.MapGet("method/list", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPaymentMethods(), cancellationToken);
            return Results.Ok(result);
        });

        paymentGroup.MapDelete("method", async (
            string paymentMethodId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeletePaymentMethod(paymentMethodId), cancellationToken);
            return Results.Ok();
        });

        paymentGroup.MapGet("tax", async (
            decimal baseAmount,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var rate = await mediator.Send(new GetTaxRate(baseAmount), cancellationToken);
            return Results.Ok(rate);
        });
    }
}

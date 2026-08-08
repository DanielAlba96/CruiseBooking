using Carter;
using Core.Api.Common;
using Core.Application;
using Core.Infrastructure;
using Core.Agents;
using Scalar.AspNetCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddKeycloakAuthentication(
    builder.Configuration["Keycloak:Authority"],
    builder.Configuration["Keycloak:Audience"],
    builder.Environment.IsProduction());

builder.AddCoreInfrastructureServices();

builder.Services.AddCoreApplicationServices();
builder.AddCoreAgentServices();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddCarter();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource("CruiseAssistant"));

builder.Logging.AddConsole();

var app = builder.Build();

app.ApplyMigrations();

app.MapDefaultEndpoints();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("CruiseBooking API")
            .AddPreferredSecuritySchemes("Bearer");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseUserInfoValidation();

app.MapCarter();

await app.RunAsync();

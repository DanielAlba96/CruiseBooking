using Shared.Persistence;
using Cruises.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddCruiseGraphQL();
builder.Services.AddMemoryCache();
builder.AddDataServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGraphQL();

await app.RunWithGraphQLCommandsAsync(args);

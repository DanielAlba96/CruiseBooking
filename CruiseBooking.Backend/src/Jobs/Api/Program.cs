using Dapr.Jobs.Extensions;
using Jobs.Application;
using Jobs.Infrastructure;
using PdfSharp.Fonts;
using Shared.Persistence;
using Jobs.Application.Jobs;
using Dapr.Jobs;
using System.Text.Json;
using Dapr.Jobs.Models;
using Shared.Domain.Models;

GlobalFontSettings.UseWindowsFontsUnderWindows = true;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDataServices();

builder.Services.AddJobsApplicationServices();
builder.Services.AddJobsInfrastructureServices(builder.Configuration);

builder.Services.AddDaprJobsClient();

var app = builder.Build();

app.UseCloudEvents();

app.MapDefaultEndpoints();

app.MapDaprScheduledJobHandler(
    async (string jobName, ReadOnlyMemory<byte> payload, IJobResolver resolver, ILogger<Program> logger, CancellationToken cancellationToken) =>
    {
        var jobId = jobName.Split(':')[0];
        var job = resolver.Resolve(jobId);
        if (job is null)
        {
            logger.LogWarning("Received trigger invocation for unknown scheduled job '{JobName}'.", jobName);
            return;
        }

        await job.ExecuteAsync(payload, cancellationToken);
    },
    TimeSpan.FromMinutes(2));

app.MapPost("/jobs", async (
    ScheduleJobRequest? message,
    DaprJobsClient daprJobService,
    ILogger<ScheduleJobRequest> logger,
    CancellationToken cancellationToken) =>
{
    if (message is null
        || string.IsNullOrWhiteSpace(message.JobName)
        || message.Payload is null)
    {
        logger.LogWarning("Dropping job request with invalid data.");
        return Results.Ok();
    }

    await daprJobService.ScheduleJobAsync(
        $"{message.JobName}:{Guid.NewGuid()}",
        DaprJobSchedule.FromDateTime(message.DueTime ?? DateTimeOffset.UtcNow),
        JsonSerializer.SerializeToUtf8Bytes(message.Payload),
        cancellationToken: cancellationToken);

    return Results.Ok();
});

await app.RunAsync();

using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Shared.Domain.Models;
using Shared.Domain.Services;

namespace Core.Infrastructure.Services;

/// <summary>
/// Servicio para encolar trabajos en segundo plano usando Dapr Jobs.
/// </summary>

internal class DaprJobService(DaprClient daprClient, IConfiguration configuration) : IJobService
{
    readonly DaprClient _daprClient = daprClient;
    readonly IConfiguration _configuration = configuration;

    internal const string PUBSUB_NAME = "jobs-pub-sub";
    internal const string TOPIC_NAME = "jobs";
    internal const string LockCabinsCleanupJobName = "lock-cabins-cleanup";
    internal const string GenerateInvoiceJobName = "generate-invoice";
    internal const string SendEmailJobName = "send-email";

    /// <inheritdoc />
    public async Task ScheduleLockedCabinCleanUp(IReadOnlyList<int> lockedCabinsIds)
    {
        var expirationMinutes = int.TryParse(_configuration["LockedCabins:ExpirationMinutes"], out var minutes) ? minutes : 10;
        var dueTime = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        var payload = JsonSerializer.SerializeToElement(new LockCabinsCleanupData(lockedCabinsIds));
        var request = new ScheduleJobRequest(LockCabinsCleanupJobName, dueTime, JsonSerializer.SerializeToElement(payload));
        await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, request);
    }

    /// <inheritdoc />
    public async Task EnqueueInvoiceAsync(int bookingId)
    {
        var payload = JsonSerializer.SerializeToElement(new InvoiceJobData(bookingId));
        var request = new ScheduleJobRequest(GenerateInvoiceJobName, null, payload);
        await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, request);
    }

    /// <inheritdoc />
    public async Task EnqueueEmailAsync(EmailData message)
    {
        var payload = JsonSerializer.SerializeToElement(message);
        var request = new ScheduleJobRequest(SendEmailJobName, null, payload);
        await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, request);
    }
}

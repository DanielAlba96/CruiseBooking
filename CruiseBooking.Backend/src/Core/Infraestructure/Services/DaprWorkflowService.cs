using Dapr.Workflow;
using Shared.Domain.Services;
using Core.Infrastructure.Workflows;

namespace Core.Infrastructure.Services;

/// <summary>
/// Define el servicio para orquestar flujos de trabajo de reservas usando Dapr Workflows.
/// </summary>
public class DaprWorkflowService(DaprWorkflowClient daprWorkflowClient) : IWorkflowService
{
    readonly DaprWorkflowClient _daprWorkflowClient = daprWorkflowClient;

    /// <inheritdoc />
    public async Task StartBookingWorkflow(int bookingId)
        => await _daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(BookingWorkflow), bookingId.ToString(), bookingId);

    /// <inheritdoc />
    public async Task SendBookingCanceledEvent(int bookingId)
       => await _daprWorkflowClient.RaiseEventAsync(bookingId.ToString(), "booking-canceled");

    /// <inheritdoc />
    public async Task SendManualPaymentEvent(int bookingId)
        => await _daprWorkflowClient.RaiseEventAsync(bookingId.ToString(), "manual-payment-completed");

    /// <inheritdoc />
    public async Task SendCheckInCompletedEvent(int bookingId)
        => await _daprWorkflowClient.RaiseEventAsync(bookingId.ToString(), "checkin-completed");
}

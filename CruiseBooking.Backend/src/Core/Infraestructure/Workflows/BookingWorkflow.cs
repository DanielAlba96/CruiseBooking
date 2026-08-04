using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using Shared.Domain.Services;
using Core.Infrastructure.Workflows.Activities;

namespace Core.Infrastructure.Workflows;

/// <summary>
/// Resultados posibles de la ejecución del flujo de trabajo de reserva.
/// </summary>
enum BookingWorkflowResults
{
    /// <summary>Flujo completado exitosamente.</summary>
    Completed = 0,
    /// <summary>Reserva cancelada durante el flujo.</summary>
    Canceled = 1,
    /// <summary>Flujo fallido por error.</summary>
    Failed = 2
}

/// <summary>
/// Datos necesarios para la ejecución del flujo de trabajo de reserva.
/// </summary>
/// <param name="Id">Identificador de la reserva.</param>
/// <param name="ChargeDate">Fecha en la que se debe efectuar el cobro de pago.</param>
/// <param name="CheckInStartDate">Fecha de inicio de check-in disponible para la reserva.</param>
sealed record BookingWorkflowData(int Id, DateTime ChargeDate, DateTime CheckInStartDate);

/// <summary>
/// Flujo de trabajo de Dapr que orquesta el ciclo completo de una reserva de crucero,
/// desde el pago hasta la confirmación de check-in.
/// </summary>
internal class BookingWorkflow : Workflow<int, BookingWorkflowResults>
{
    /// <summary>
    /// Ejecuta el flujo de trabajo completo de la reserva.
    /// Maneja el proceso de pago, check-in y cancelación según eventos externos y timers.
    /// </summary>
    /// <param name="context">Contexto de ejecución del flujo de trabajo Dapr.</param>
    /// <param name="input">Identificador de la reserva a procesar.</param>
    /// <returns>Resultado final del flujo: completado, cancelado o fallido.</returns>
    public override async Task<BookingWorkflowResults> RunAsync(WorkflowContext context, int input)
    {
        ILogger logger = context.CreateReplaySafeLogger<BookingWorkflow>();

        var retry = new WorkflowTaskOptions
        {
            RetryPolicy = new WorkflowRetryPolicy(
                maxNumberOfAttempts: 5,
                firstRetryInterval: TimeSpan.FromSeconds(30),
                backoffCoefficient: 2)
        };

        try
        {
            var booking = await context.CallActivityAsync<BookingWorkflowData>(nameof(GetBookingActivity), input, retry);

            var paymentTimer = context.CreateTimer(booking.ChargeDate, CancellationToken.None);
            var paymentBookingCanceled = context.WaitForExternalEventAsync<object>("booking-canceled");
            var manualEarlyPayment = context.WaitForExternalEventAsync<object>("manual-payment-completed");
            var winnerPayment = await Task.WhenAny(paymentBookingCanceled, manualEarlyPayment, paymentTimer);

            if (winnerPayment == paymentBookingCanceled)
            {
                await context.CallActivityAsync(nameof(SendBookingCanceledEmailActivity), input, retry);
                return BookingWorkflowResults.Canceled;
            }

            if (winnerPayment == paymentTimer)
            {
                var chargeResult = await context.CallActivityAsync<ChargeResult>(nameof(CompletePaymentActivity), input);

                if (chargeResult is null || !chargeResult.Success)
                {
                    await context.CallActivityAsync(nameof(SendPaymentErrorEmailActivity), input, retry);
                    var manualPayment = context.WaitForExternalEventAsync<object>("manual-payment-completed");
                    var bookingCanceled = context.WaitForExternalEventAsync<object>("booking-canceled");
                    var deadline = context.CreateTimer(booking.CheckInStartDate, CancellationToken.None);
                    var winner = await Task.WhenAny(manualPayment, bookingCanceled, deadline);

                    if (winner == bookingCanceled)
                    {
                        await context.CallActivityAsync(nameof(SendBookingCanceledEmailActivity), input, retry);
                        return BookingWorkflowResults.Canceled;
                    }

                    if (winner == deadline)
                    {
                        await context.CallActivityAsync(nameof(CancelUnpaidBookingActivity), input, retry);
                        await context.CallActivityAsync(nameof(SendPaymentDeadlineEmailActivity), input, retry);
                        return BookingWorkflowResults.Canceled;
                    }
                }
            }

            await context.CallActivityAsync(nameof(EnqueueInvoiceActivity), input, retry);

            await context.CreateTimer(booking.CheckInStartDate, CancellationToken.None);
            await context.CallActivityAsync(nameof(SendCheckInEmailActivity), input, retry);

            var checkInBookingCanceled = context.WaitForExternalEventAsync<object>("booking-canceled");
            var checkInCompleted = context.WaitForExternalEventAsync<object>("checkin-completed");
            var winnerCheckIn = await Task.WhenAny(checkInBookingCanceled, checkInCompleted);
            if (winnerCheckIn == checkInBookingCanceled)
            {
                await context.CallActivityAsync(nameof(SendBookingCanceledEmailActivity), input, retry);
                return BookingWorkflowResults.Canceled;
            }

            return BookingWorkflowResults.Completed;
        }
        catch (WorkflowTaskFailedException e)
        {
            logger.LogError(
                e,
                "El workflow de la reserva {BookingId} (instancia {InstanceId}) ha fallado en {ErrorType}: {ErrorMessage}",
                input,
                context.InstanceId,
                e.FailureDetails.ErrorType,
                e.FailureDetails.ErrorMessage);

            return BookingWorkflowResults.Failed;
        }
    }
}

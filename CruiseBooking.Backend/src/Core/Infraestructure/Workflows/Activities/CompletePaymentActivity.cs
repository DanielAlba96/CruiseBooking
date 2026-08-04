using Core.Application.Common.Exceptions;
using Core.Application.Resources;
using Dapr.Workflow;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Infrastructure.Workflows.Activities;

/// <summary>
/// Actividad de flujo de trabajo que procesa el cobro de pago para una reserva de crucero.
/// Valida que la reserva y el método de pago existan, ejecuta el cobro mediante el servicio de pagos
/// y marca la reserva como pagada si la transacción es exitosa.
/// </summary>
internal class CompletePaymentActivity(IPaymentService paymentService, IBookingRepository bookingRepository)
    : WorkflowActivity<int, ChargeResult>
{
    readonly IPaymentService _paymentService = paymentService;
    readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <summary>
    /// Ejecuta el proceso de cobro de pago para la reserva especificada.
    /// </summary>
    /// <param name="context">Contexto de ejecución del flujo de trabajo de Dapr.</param>
    /// <param name="input">Identificador de la reserva (BookingId) a procesar.</param>
    /// <returns>
    /// Un objeto <see cref="ChargeResult"/> que indica si el cobro fue exitoso (Success),
    /// el identificador de la transacción (ChargeId), el monto cobrado (ChargeAmount)
    /// y un mensaje descriptivo del resultado.
    /// </returns>
    /// <exception cref="NotFoundException">Se lanza si la reserva especificada no existe en la base de datos.</exception>
    public override async Task<ChargeResult> RunAsync(WorkflowActivityContext context, int input)
    {
        var booking = await _bookingRepository.GetBookingPaymentData(input)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFoundInWorkflow, input));

        if (booking.IsCanceled)
        {
            return new ChargeResult(false, string.Empty, 0.0m, "La reserva está cancelada.");
        }

        if (booking.CustomerId is null || booking.PaymentMethodId is null)
        {
            await _bookingRepository.MarkPaymentAsFailed(booking.BookingId);
            return new ChargeResult(false, string.Empty, 0.0m, "El usuario no tiene un método de pago activo.");
        }

        var result = await _paymentService.ChargeAsync(
            booking.CustomerId,
            booking.PaymentMethodId,
            booking.Subtotal);

        if (!result.Success)
        {
            await _bookingRepository.MarkPaymentAsFailed(booking.BookingId);
            return result;
        }

        var chargedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        await _bookingRepository.MarkBookingAsPaid(booking.BookingId, result.ChargeId, booking.PaymentMethodId, result.ChargeAmount, chargedAt);

        return result;
    }
}

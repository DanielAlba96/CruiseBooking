using System.Globalization;
using Microsoft.Extensions.Logging;
using Shared.Domain.Models;
using Shared.Domain.Services;

namespace Core.Infrastructure.Services;

/// <summary>
/// Implementación falsa de <see cref="IPaymentService"/> para entornos sin pasarela de pago real.
/// Todas las operaciones se resuelven correctamente y el impuesto aplicado es siempre del 21%.
/// </summary>
internal class FakePaymentService(ILoggerFactory loggerFactory) : IPaymentService
{
    /// <summary>Tipo impositivo por defecto aplicado por el servicio falso.</summary>
    private const decimal TaxPercentage = 21.0m;

    private readonly ILogger<FakePaymentService> _logger = loggerFactory.CreateLogger<FakePaymentService>();

    /// <inheritdoc />
    public Task<string?> CreateCustomer(int userDbId, string userCountry, string userFullName, string email)
    {
        var customerId = $"cus_fake_{userDbId}";
        _logger.LogInformation("FakePaymentService: cliente {CustomerId} creado para el usuario {UserDbId}", customerId, userDbId);
        return Task.FromResult<string?>(customerId);
    }

    /// <inheritdoc />
    public async Task<ChargeResult> ChargeAsync(string customerId, string paymentMethodId, decimal amount)
    {
        var taxedAmount = await GetAmountWithTaxesAsync(customerId, amount);
        var chargeId = $"ch_fake_{Guid.NewGuid():N}";

        _logger.LogInformation(
            "FakePaymentService: cobro {ChargeId} de {Amount} EUR al cliente {CustomerId}",
            chargeId,
            taxedAmount.TotalAmount,
            customerId);

        return new ChargeResult(true, chargeId, taxedAmount.TotalAmount);
    }

    /// <inheritdoc />
    public Task<List<PaymentMethodResponse>> GetPaymentMethods(string customerId)
    {
        List<PaymentMethodResponse> result =
        [
            new PaymentMethodResponse
            {
                Id = $"pm_fake_{customerId}",
                Type = "card",
                Card = new PaymentMethodCardModel
                {
                    Brand = "visa",
                    Country = "ES",
                    Description = "Tarjeta de prueba",
                    ExpMonth = 12,
                    ExpYear = 2030,
                    Issuer = "Fake Bank",
                    Last4 = "4242"
                }
            }
        ];

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<SetupIntentResult> AddPaymentMethod(string customerId)
    {
        var clientSecret = $"seti_fake_{Guid.NewGuid():N}_secret";
        return Task.FromResult(new SetupIntentResult(true, clientSecret));
    }

    /// <inheritdoc />
    public Task DeletePaymentMethod(string paymentMethodId)
    {
        _logger.LogInformation("FakePaymentService: método de pago {PaymentMethodId} eliminado", paymentMethodId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<RefundResult> RefundAsync(string chargeId, string reason)
    {
        var refundId = $"re_fake_{Guid.NewGuid():N}";
        _logger.LogInformation(
            "FakePaymentService: reembolso {RefundId} del cargo {ChargeId} (motivo {Reason})",
            refundId,
            chargeId,
            reason);

        return Task.FromResult(new RefundResult(true, refundId));
    }

    /// <inheritdoc />
    public Task<TaxRateResult> GetAmountWithTaxesAsync(string customerId, decimal baseAmount)
    {
        var totalAmount = Math.Round(baseAmount * (1 + (TaxPercentage / 100m)), 2, MidpointRounding.AwayFromZero);
        var percentage = TaxPercentage.ToString("0.00", CultureInfo.InvariantCulture);

        return Task.FromResult(new TaxRateResult(true, totalAmount, percentage));
    }
}

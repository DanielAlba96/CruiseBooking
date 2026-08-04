using Core.Application.Common;
using Microsoft.Extensions.Logging;
using Shared.Domain.Models;
using Shared.Domain.Services;
using Stripe;
using Stripe.Tax;

namespace Core.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar operaciones de pago usando Stripe
/// </summary>
internal class StripePaymentService(ILoggerFactory loggerFactory) : IPaymentService
{
    readonly ILogger<StripePaymentService> _logger = loggerFactory.CreateLogger<StripePaymentService>();

    /// <inheritdoc />
    public async Task<ChargeResult> ChargeAsync(string customerId, string paymentMethodId, decimal amount)
    {
        try
        {
            var taxedAmount = await GetAmountWithTaxesAsync(customerId, amount);

            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Currency = "eur",
                Amount = taxedAmount.TotalAmount.ToStripeAmount(),
                Customer = customerId,
                PaymentMethod = paymentMethodId,
                Confirm = true,
                OffSession = true
            };

            var charge = await service.CreateAsync(options);
            return new ChargeResult(true, charge.LatestChargeId, charge.Amount.ToEuros());
        }
        catch (StripeException e)
        {
            _logger.LogError(
               e,
               "Error procesando el pago al medio {PaymentMethodId} al cliente {CustomerId}",
               paymentMethodId,
               customerId);

            return new ChargeResult(false, string.Empty, 0.0m, "Error procesando el pago");
        }
    }

    /// <inheritdoc />
    public async Task<string?> CreateCustomer(int userDbId, string userCountry, string userFullName, string email)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = userFullName,
                Address = new AddressOptions
                {
                    Country = userCountry
                },
                Metadata = new Dictionary<string, string>()
                {
                    { "ID", userDbId.ToString() }
                }
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);
            return customer.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during customer creation for userId {UserDbId}", userDbId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<PaymentMethodResponse>> GetPaymentMethods(string customerId)
    {
        var options = new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card"
        };

        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(options);

        List<PaymentMethodResponse> result = [];
        foreach (var stripePaymentMethod in paymentMethods)
        {
            PaymentMethodResponse currentPaymentMethod = new PaymentMethodResponse
            {
                Id = stripePaymentMethod.Id,
                Type = stripePaymentMethod.Type,
                Card = new PaymentMethodCardModel()
                {
                    Brand = stripePaymentMethod.Card.Brand,
                    Country = stripePaymentMethod.Card.Country,
                    ExpMonth = stripePaymentMethod.Card.ExpMonth,
                    ExpYear = stripePaymentMethod.Card.ExpYear,
                    Issuer = stripePaymentMethod.Card.Issuer,
                    Last4 = stripePaymentMethod.Card.Last4,
                    Description = stripePaymentMethod.Card.Description
                }
            };

            result.Add(currentPaymentMethod);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<SetupIntentResult> AddPaymentMethod(string customerId)
    {
        try
        {
            var service = new SetupIntentService();
            var options = new SetupIntentCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = ["card"],
                Usage = "off_session"
            };

            var setupIntent = await service.CreateAsync(options);
            return new SetupIntentResult(true, setupIntent.ClientSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "An error occurred creating the SetupIntent for customer {CustomerId}", customerId);
            return new SetupIntentResult(false, string.Empty, "Error creando el SetupIntent");
        }
    }

    /// <inheritdoc />
    public async Task DeletePaymentMethod(string paymentMethodId)
    {
        var service = new PaymentMethodService();
        await service.DetachAsync(paymentMethodId);
    }

    /// <inheritdoc />
    public async Task<TaxRateResult> GetAmountWithTaxesAsync(string customerId, decimal baseAmount)
    {
        var service = new CalculationService();
        var options = new CalculationCreateOptions
        {
            Customer = customerId,
            Currency = "eur",
            LineItems = new List<CalculationLineItemOptions>
            {
                new CalculationLineItemOptions
                {
                    Amount = baseAmount.ToStripeAmount(),
                    Quantity = 1,
                    Reference = Guid.NewGuid().ToString()
                }
            }
        };

        var taxResponse = await service.CreateAsync(options);
        if (taxResponse.TaxBreakdown.Count != 1)
        {
            return new TaxRateResult(false, baseAmount, "0.00", "No se pudo calcular el impuesto");
        }

        var tax = taxResponse.TaxBreakdown.Single();
        return new TaxRateResult(true, taxResponse.AmountTotal.ToEuros(), tax.TaxRateDetails.PercentageDecimal);
    }

    /// <inheritdoc />
    public async Task<RefundResult> RefundAsync(string chargeId, string reason)
    {
        try
        {
            var service = new RefundService();
            var options = new RefundCreateOptions
            {
                Charge = chargeId,
                Reason = reason,
                Amount = null // Refund the full amount
            };

            var refund = await service.CreateAsync(options);
            return new RefundResult(true, refund.Id);
        }
        catch (StripeException e)
        {
            _logger.LogError(
                e,
                "No se ha podido reembolsar el cargo {ChargeId} (motivo {Reason}). El cliente sigue cobrado.",
                chargeId,
                reason);

            return new RefundResult(false, string.Empty, "Error reembolsando el cargo");
        }
    }
}

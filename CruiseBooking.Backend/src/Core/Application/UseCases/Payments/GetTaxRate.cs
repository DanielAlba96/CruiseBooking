using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Payments;

/// <summary>Calcula el importe total con impuestos para el usuario actual a partir de una base imponible.</summary>
/// <param name="BaseAmount">Base imponible sobre la que calcular el impuesto.</param>
public sealed record GetTaxRate(decimal BaseAmount) : IRequest<TaxRateResponse>;

/// <summary>Atiende <see cref="GetTaxRate"/>.</summary>
/// <param name="paymentService">Pasarela de pago, que resuelve el tipo impositivo según el país del cliente.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class GetTaxRateHandler(
    IPaymentService paymentService,
    UserInfo userInfo) : IRequestHandler<GetTaxRate, TaxRateResponse>
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<TaxRateResponse> Handle(GetTaxRate request, CancellationToken cancellationToken)
    {
        var customerId = _userInfo.CustomerId
            ?? throw new UnauthorizedException(ErrorMessages.PaymentCustomerMissing);

        var rate = await _paymentService.GetAmountWithTaxesAsync(customerId, request.BaseAmount);
        if (!rate.Success)
        {
            throw new ValidationException(rate.ErrorMessage ?? ErrorMessages.TaxCalculationFailed);
        }

        return new TaxRateResponse(rate.TotalAmount, rate.TaxPercentage);
    }
}

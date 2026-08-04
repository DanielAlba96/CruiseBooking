using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Models;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Payments;

/// <summary>Obtiene los medios de pago dados de alta por el usuario actual.</summary>
public sealed record GetPaymentMethods : IRequest<List<PaymentMethodResponse>>;

/// <summary>Atiende <see cref="GetPaymentMethods"/>.</summary>
/// <param name="paymentService">Pasarela de pago.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class GetPaymentMethodsHandler(
    IPaymentService paymentService,
    UserInfo userInfo) : IRequestHandler<GetPaymentMethods, List<PaymentMethodResponse>>
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<List<PaymentMethodResponse>> Handle(GetPaymentMethods request, CancellationToken cancellationToken)
    {
        var customerId = _userInfo.CustomerId
            ?? throw new UnauthorizedException(ErrorMessages.PaymentCustomerMissing);

        return await _paymentService.GetPaymentMethods(customerId);
    }
}

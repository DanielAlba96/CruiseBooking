using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Payments;

/// <summary>Inicia el alta de un medio de pago para el usuario actual en la pasarela de pago.</summary>
public sealed record AddPaymentMethod : IRequest<AddPaymentMethodResponse>;

/// <summary>Atiende <see cref="AddPaymentMethod"/>.</summary>
/// <param name="paymentService">Pasarela de pago.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class AddPaymentMethodHandler(
    IPaymentService paymentService,
    UserInfo userInfo) : IRequestHandler<AddPaymentMethod, AddPaymentMethodResponse>
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<AddPaymentMethodResponse> Handle(AddPaymentMethod request, CancellationToken cancellationToken)
    {
        var customerId = _userInfo.CustomerId
            ?? throw new UnauthorizedException(ErrorMessages.PaymentCustomerMissing);

        var result = await _paymentService.AddPaymentMethod(customerId);

        return new AddPaymentMethodResponse(result.ClientSecret);
    }
}

using System.Text.Json;
using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Users;

/// <summary>Crea o actualiza la ficha local del usuario autenticado y le asocia una ficha de cliente en la pasarela de pago.</summary>
/// <param name="Request">Datos de perfil enviados por el usuario.</param>
public sealed record CreateOrUpdateUser(CreateOrUpdateUserRequest Request) : IRequest;

/// <summary>Atiende <see cref="CreateOrUpdateUser"/>.</summary>
/// <param name="userRepository">Repositorio de usuarios.</param>
/// <param name="paymentService">Pasarela de pago, usada para dar de alta la ficha de cliente.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
/// <remarks>
/// Es la única ruta que se atiende antes de que exista la fila del usuario en base de datos, por lo que
/// solo puede leerse <see cref="UserInfo.KeycloakGuid"/> de <paramref name="userInfo"/>; el resto de campos
/// todavía no están poblados.
/// </remarks>
public sealed class CreateOrUpdateUserHandler(
    IUserRepository userRepository,
    IPaymentService paymentService,
    UserInfo userInfo) : IRequestHandler<CreateOrUpdateUser>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPaymentService _paymentService = paymentService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(CreateOrUpdateUser request, CancellationToken cancellationToken = default)
    {
        var addressDto = JsonSerializer.Deserialize<AddressDto>(request.Request.Address);

        var user = await _userRepository.CreateOrUpdateUser(
            _userInfo.KeycloakGuid,
            request.Request.FirstName,
            request.Request.LastName,
            request.Request.Email,
            request.Request.Phone,
            new Address
            {
                Street = addressDto?.Street ?? string.Empty,
                Locality = addressDto?.Locality ?? string.Empty,
                Region = addressDto?.Region ?? string.Empty,
                PostalCode = addressDto?.PostalCode ?? string.Empty,
                Country = addressDto?.Country ?? string.Empty
            });

        if (user.CustomerId is null)
        {
            var customerId = await _paymentService.CreateCustomer(user.Id, user.Address.Country, $"{user.Name} {user.Surname}", user.Email);
            if (customerId is not null)
            {
                await _userRepository.UpdateCustomerId(user, customerId);
            }
        }
    }
}

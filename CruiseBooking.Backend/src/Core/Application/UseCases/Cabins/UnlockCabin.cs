using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.Cabins;

/// <summary>Libera una cabina bloqueada del usuario actual.</summary>
/// <param name="CruiseDateId">Identificador de la fecha de crucero.</param>
/// <param name="CabinId">Identificador de la cabina del barco.</param>
public sealed record UnlockCabin(int CruiseDateId, int CabinId) : IRequest;

/// <summary>Atiende <see cref="UnlockCabin"/>.</summary>
/// <param name="cabinRepository">Repositorio de camarotes.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class UnlockCabinHandler(
    ICabinRepository cabinRepository,
    UserInfo userInfo) : IRequestHandler<UnlockCabin>
{
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(UnlockCabin request, CancellationToken cancellationToken)
    {
        await _cabinRepository.UnlockCabin(request.CruiseDateId, request.CabinId, _userInfo.Id);
    }
}

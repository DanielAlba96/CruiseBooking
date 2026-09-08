using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.Cabins;

/// <summary>Obtiene los bloqueos de camarote vigentes del usuario actual para una fecha de crucero.</summary>
/// <param name="CruiseDateId">Identificador de la fecha de crucero.</param>
public sealed record GetLockedCabins(int CruiseDateId) : IRequest<IReadOnlyList<GetLockedCabinDtoResponse>>;

/// <summary>Atiende <see cref="GetLockedCabins"/>.</summary>
/// <param name="cabinRepository">Repositorio de camarotes.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class GetLockedCabinsHandler(
    ICabinRepository cabinRepository,
    UserInfo userInfo) : IRequestHandler<GetLockedCabins, IReadOnlyList<GetLockedCabinDtoResponse>>
{
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<IReadOnlyList<GetLockedCabinDtoResponse>> Handle(GetLockedCabins request, CancellationToken cancellationToken)
    {
        var lockedCabins = await _cabinRepository.GetLockedCabins(request.CruiseDateId, _userInfo.Id);

        return [.. lockedCabins.Select(l => new GetLockedCabinDtoResponse
        {
            Id = l.Id,
            CabinId = l.CabinId,
            Occupants = l.Occupants,
            Price = l.Price,
            LastUpdatedAt = l.LastUpdatedAt
        })];
    }
}

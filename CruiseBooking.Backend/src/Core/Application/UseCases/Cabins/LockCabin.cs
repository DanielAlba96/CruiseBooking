using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Cabins;

/// <summary>Bloquea una única cabina de una fecha de crucero para el usuario actual y programa su liberación automática.</summary>
/// <param name="CruiseDateId">Identificador de la fecha de crucero.</param>
/// <param name="CabinId">Identificador de la cabina del barco.</param>
/// <param name="Occupants">Número de pasajeros que ocuparán la cabina.</param>
public sealed record LockCabin(int CruiseDateId, int CabinId, int Occupants) : IRequest<int>;

/// <summary>Atiende <see cref="LockCabin"/>.</summary>
/// <param name="cabinRepository">Repositorio de camarotes.</param>
/// <param name="jobService">Servicio de tareas programadas, usado para liberar la cabina bloqueada.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class LockCabinHandler(
    ICabinRepository cabinRepository,
    IJobService jobService,
    UserInfo userInfo) : IRequestHandler<LockCabin, int>
{
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IJobService _jobService = jobService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<int> Handle(LockCabin request, CancellationToken cancellationToken = default)
    {
        var cabin = await _cabinRepository.GetCabinById(request.CabinId)
            ?? throw new NotFoundException("No se encuentra el camarote");

        var lockedCabinId = await _cabinRepository.LockCabin(request.CruiseDateId, _userInfo.Id, cabin.Id, cabin.Price, request.Occupants);
        await _jobService.ScheduleLockedCabinCleanUp([lockedCabinId]);
        return lockedCabinId;
    }
}

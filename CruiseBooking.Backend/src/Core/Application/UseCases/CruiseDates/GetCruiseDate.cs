using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CruiseDates;

/// <summary>Obtiene una fecha de crucero con sus camarotes y extras disponibles.</summary>
/// <param name="CruiseDateId">Identificador de la fecha de crucero.</param>
public sealed record GetCruiseDate(int CruiseDateId) : IRequest<GetCruiseDateResponse>;

/// <summary>Atiende <see cref="GetCruiseDate"/>.</summary>
/// <param name="cruiseRepository">Repositorio de cruceros.</param>
/// <param name="cabinRepository">Repositorio de camarotes.</param>
public sealed class GetCruiseDateHandler(
    ICruiseRepository cruiseRepository,
    ICabinRepository cabinRepository) : IRequestHandler<GetCruiseDate, GetCruiseDateResponse>
{
    private readonly ICruiseRepository _cruiseRepository = cruiseRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;

    /// <inheritdoc />
    public async Task<GetCruiseDateResponse> Handle(GetCruiseDate request, CancellationToken cancellationToken)
    {
        var cruiseDate = await _cruiseRepository.GetCruiseDateWithExtras(request.CruiseDateId);
        var cabins = await _cabinRepository.GetCabinsByCruiseDate(request.CruiseDateId);

        return new GetCruiseDateResponse(
            Name: cruiseDate.Cruise!.Name,
            OriginPort: cruiseDate.Cruise.OriginPort,
            StartDate: cruiseDate.StartDate,
            DurationInDays: cruiseDate.Cruise.DurationInDays,
            AvailableCabins: cabins
                .Select(c => new CruiseDateCabinResponse(
                    Id: c.Id,
                    Name: c.CabinType?.Name ?? string.Empty,
                    Description: c.CabinType?.Description ?? string.Empty,
                    MaxOccupancy: c.CabinType?.MaxOccupancy ?? 0,
                    AvailableCount: c.Number,
                    FullPrice: c.Price))
                .ToList(),
            AvailableExtras: cruiseDate.CruiseDateExtras!
                .Select(e => new CruiseDateExtraResponse(
                    Id: e.ExtraId,
                    Name: e.Extra?.Name ?? string.Empty,
                    Description: e.Extra?.Description ?? string.Empty,
                    Price: e.Price))
                .ToList());
    }
}

using Core.Application.Common.CQRS;
using Core.Application.Models;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.Bookings;

/// <summary>Obtiene el listado resumido de las reservas del usuario actual.</summary>
public sealed record GetBookingsSummary : IRequest<IReadOnlyList<GetCurrentUserBookingSummaryDtoResponse>>;

/// <summary>Atiende <see cref="GetBookingsSummary"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class GetBookingsSummaryHandler(
    IBookingRepository bookingRepository,
    UserInfo userInfo) : IRequestHandler<GetBookingsSummary, IReadOnlyList<GetCurrentUserBookingSummaryDtoResponse>>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<IReadOnlyList<GetCurrentUserBookingSummaryDtoResponse>> Handle(GetBookingsSummary request, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetBookingsByUserId(_userInfo.Id);

        return bookings.Select(b => new GetCurrentUserBookingSummaryDtoResponse
        {
            Id = b.Id,
            IsCanceled = b.Status == BookingStatus.Canceled,
            Status = b.Status,
            Name = b.CruiseDate?.Cruise?.Name ?? string.Empty,
            Description = b.CruiseDate?.Cruise?.Description ?? string.Empty,
            Destination = $"{b.CruiseDate?.Cruise?.Zone} - salida desde {b.CruiseDate?.Cruise?.OriginPort}",
            StartDate = b.CruiseDate?.StartDate ?? DateTime.MinValue,
            EndDate = (b.CruiseDate?.StartDate ?? DateTime.MinValue).AddDays(b.CruiseDate?.Cruise?.DurationInDays ?? 0),
            PassengerCount = b.Cabins?.Sum(c => c.Occupants) ?? 0
        }).ToList();
    }
}

using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.Bookings;

/// <summary>Obtiene el detalle de una reserva del usuario actual.</summary>
/// <param name="BookingId">Identificador de la reserva.</param>
public sealed record GetBookingDetail(int BookingId) : IRequest<GetCurrentUserBookingDetailDtoResponse>;

/// <summary>Atiende <see cref="GetBookingDetail"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class GetBookingDetailHandler(
    IBookingRepository bookingRepository,
    UserInfo userInfo) : IRequestHandler<GetBookingDetail, GetCurrentUserBookingDetailDtoResponse>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<GetCurrentUserBookingDetailDtoResponse> Handle(GetBookingDetail request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetBookingByIdAndUserId(request.BookingId, _userInfo.Id)
            ?? throw new NotFoundException(string.Format(ErrorMessages.BookingNotFound, request.BookingId));

        return MapBookingDetail(booking);
    }

    private static GetCurrentUserBookingDetailDtoResponse MapBookingDetail(Booking booking)
    {
        var cruise = booking.CruiseDate?.Cruise;
        var ship = booking.CruiseDate?.Ship;

        return new GetCurrentUserBookingDetailDtoResponse
        {
            Id = booking.Id,
            IsCanceled = booking.Status == BookingStatus.Canceled,
            Status = booking.Status,
            Name = cruise?.Name ?? string.Empty,
            Code = cruise?.Code ?? string.Empty,
            Description = cruise?.Description ?? string.Empty,
            Destination = cruise is null ? string.Empty : $"{cruise.Zone} - salida desde {cruise.OriginPort}",
            OriginPort = cruise?.OriginPort ?? string.Empty,
            Itinerary = cruise?.Itinerary ?? string.Empty,
            StartDate = booking.CruiseDate?.StartDate ?? DateTime.MinValue,
            EndDate = (booking.CruiseDate?.StartDate ?? DateTime.MinValue).AddDays(cruise?.DurationInDays ?? 0),
            PaymentStartDate = booking.CruiseDate?.PaymentStartDate ?? DateTime.MinValue,
            CheckInStartDate = booking.CruiseDate?.CheckInStartDate ?? DateTime.MinValue,
            DurationInDays = cruise?.DurationInDays ?? 0,
            ShipName = ship?.Name ?? string.Empty,
            ShipCompany = ship?.Company ?? string.Empty,
            PassengerCount = booking.Cabins?.Sum(c => c.Occupants) ?? 0,
            CreatedAt = booking.CreatedAt,
            ChargedAt = booking.ChargedAt,
            RefundedAt = booking.RefundedAt,
            Cabins = [.. booking.Cabins?.Select(bc => new GetCurrentUserBookingCabinDtoResponse
            {
                Id = bc.Id,
                TypeName = bc.Cabin?.CabinType?.Name ?? string.Empty,
                TypeDescription = bc.Cabin?.CabinType?.Description ?? string.Empty,
                Occupants = bc.Occupants,
                Price = bc.Price,
                Passengers = [.. bc.OccupantsDetails?.Select(occ => new GetCurrentUserBookingPassengerDtoResponse
                {
                    Name = occ.Name,
                    Surname = occ.Surname,
                    BirthDate = occ.BirthDate,
                    Email = occ.Email,
                    Phone = occ.Phone,
                    Address = occ.Address
                }) ?? []]
            }) ?? []],
            Extras = [.. booking.Extras?.Where(ex => ex.Extra != null).Select(ex => new GetCurrentUserBookingExtraDtoResponse
            {
                Id = ex.Extra!.Id,
                Name = ex.Extra.Name,
                Description = ex.Extra.Description,
                Price = ex.Price
            }) ?? []]
        };
    }
}

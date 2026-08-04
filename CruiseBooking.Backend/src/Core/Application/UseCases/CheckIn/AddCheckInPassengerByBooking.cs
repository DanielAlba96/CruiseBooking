using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Registra un pasajero en un camarote de una reserva del usuario actual.</summary>
/// <param name="BookingId">Identificador de la reserva.</param>
/// <param name="BookingCabinId">Identificador del camarote de la reserva.</param>
/// <param name="Passenger">Datos del pasajero.</param>
public sealed record AddCheckInPassengerByBooking(
    int BookingId,
    int BookingCabinId,
    BookingCabinOccupantRequest Passenger) : IRequest<int>;

/// <summary>Atiende <see cref="AddCheckInPassengerByBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class AddCheckInPassengerByBookingHandler(
    IBookingRepository bookingRepository,
    UserInfo userInfo) : IRequestHandler<AddCheckInPassengerByBooking, int>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<int> Handle(AddCheckInPassengerByBooking request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByOwner(_bookingRepository, request.BookingId, _userInfo.Id);

        return await CheckInHelpers.AddPassenger(_bookingRepository, booking, request.BookingCabinId, request.Passenger);
    }
}

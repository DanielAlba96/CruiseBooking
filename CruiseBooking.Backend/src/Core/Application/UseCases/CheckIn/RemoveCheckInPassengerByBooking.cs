using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Elimina un pasajero de una reserva del usuario actual.</summary>
/// <param name="BookingId">Identificador de la reserva.</param>
/// <param name="PassengerId">Identificador del pasajero a eliminar.</param>
public sealed record RemoveCheckInPassengerByBooking(int BookingId, int PassengerId) : IRequest;

/// <summary>Atiende <see cref="RemoveCheckInPassengerByBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class RemoveCheckInPassengerByBookingHandler(
    IBookingRepository bookingRepository,
    UserInfo userInfo) : IRequestHandler<RemoveCheckInPassengerByBooking>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(RemoveCheckInPassengerByBooking request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByOwner(_bookingRepository, request.BookingId, _userInfo.Id);

        await CheckInHelpers.RemovePassenger(_bookingRepository, booking, request.PassengerId);
    }
}

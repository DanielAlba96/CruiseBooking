using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Registra un pasajero en un camarote de la reserva identificada por el token de check-in.</summary>
/// <param name="Token">Token opaco de check-in.</param>
/// <param name="BookingCabinId">Identificador del camarote de la reserva.</param>
/// <param name="Passenger">Datos del pasajero.</param>
public sealed record AddCheckInPassengerByToken(
    string Token,
    int BookingCabinId,
    BookingCabinOccupantRequest Passenger) : IRequest<int>;

/// <summary>Atiende <see cref="AddCheckInPassengerByToken"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
public sealed class AddCheckInPassengerByTokenHandler(IBookingRepository bookingRepository)
    : IRequestHandler<AddCheckInPassengerByToken, int>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <inheritdoc />
    public async Task<int> Handle(AddCheckInPassengerByToken request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByToken(_bookingRepository, request.Token);

        return await CheckInHelpers.AddPassenger(_bookingRepository, booking, request.BookingCabinId, request.Passenger);
    }
}

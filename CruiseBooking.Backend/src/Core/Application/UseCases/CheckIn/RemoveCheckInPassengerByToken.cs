using Core.Application.Common.CQRS;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Elimina un pasajero de la reserva identificada por el token de check-in.</summary>
/// <param name="Token">Token opaco de check-in.</param>
/// <param name="PassengerId">Identificador del pasajero a eliminar.</param>
public sealed record RemoveCheckInPassengerByToken(string Token, int PassengerId) : IRequest;

/// <summary>Atiende <see cref="RemoveCheckInPassengerByToken"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
public sealed class RemoveCheckInPassengerByTokenHandler(IBookingRepository bookingRepository)
    : IRequestHandler<RemoveCheckInPassengerByToken>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <inheritdoc />
    public async Task Handle(RemoveCheckInPassengerByToken request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByToken(_bookingRepository, request.Token);

        await CheckInHelpers.RemovePassenger(_bookingRepository, booking, request.PassengerId);
    }
}

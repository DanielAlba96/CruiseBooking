using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Obtiene los datos de check-in de una reserva del usuario actual.</summary>
/// <param name="BookingId">Identificador de la reserva.</param>
public sealed record GetCheckInByBooking(int BookingId) : IRequest<GetCheckInDtoResponse>;

/// <summary>Atiende <see cref="GetCheckInByBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class GetCheckInByBookingHandler(
    IBookingRepository bookingRepository,
    UserInfo userInfo) : IRequestHandler<GetCheckInByBooking, GetCheckInDtoResponse>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task<GetCheckInDtoResponse> Handle(GetCheckInByBooking request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByOwner(_bookingRepository, request.BookingId, _userInfo.Id);

        return CheckInHelpers.MapCheckIn(booking);
    }
}

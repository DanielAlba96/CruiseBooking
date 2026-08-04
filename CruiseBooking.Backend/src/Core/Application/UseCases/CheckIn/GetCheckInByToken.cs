using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Obtiene los datos de check-in de una reserva a partir del token enviado por correo.</summary>
/// <param name="Token">Token opaco de check-in.</param>
public sealed record GetCheckInByToken(string Token) : IRequest<GetCheckInDtoResponse>;

/// <summary>Atiende <see cref="GetCheckInByToken"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
public sealed class GetCheckInByTokenHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetCheckInByToken, GetCheckInDtoResponse>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    /// <inheritdoc />
    public async Task<GetCheckInDtoResponse> Handle(GetCheckInByToken request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByToken(_bookingRepository, request.Token);

        return CheckInHelpers.MapCheckIn(booking);
    }
}

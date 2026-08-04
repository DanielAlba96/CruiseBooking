using Core.Application.Common.CQRS;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.Bookings;

/// <summary>Confirma la reserva del usuario actual a partir de los camarotes que tenga bloqueados y arranca su workflow.</summary>
/// <param name="Booking">Datos de la reserva a confirmar.</param>
public sealed record CompleteBooking(CreateBookingRequest Booking) : IRequest;

/// <summary>Atiende <see cref="CompleteBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="cabinRepository">Repositorio de camarotes, del que se leen los bloqueos vigentes.</param>
/// <param name="extraRepository">Repositorio de extras, del que se leen los precios vigentes.</param>
/// <param name="workflowService">Servicio de workflows, usado para arrancar el proceso de la reserva.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class CompleteBookingHandler(
    IBookingRepository bookingRepository,
    ICabinRepository cabinRepository,
    IExtraRepository extraRepository,
    IWorkflowService workflowService,
    UserInfo userInfo) : IRequestHandler<CompleteBooking>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly ICabinRepository _cabinRepository = cabinRepository;
    private readonly IExtraRepository _extraRepository = extraRepository;
    private readonly IWorkflowService _workflowService = workflowService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(CompleteBooking request, CancellationToken cancellationToken)
    {
        var bookingDto = request.Booking;

        var lockedCabins = await _cabinRepository.GetLockedCabins(bookingDto.CruiseDateId, _userInfo.Id);
        if (lockedCabins.Count == 0 || bookingDto.SelectedCabins.Count == 0)
        {
            throw new CabinNotAvailableException();
        }

        var matchedCabins = MatchRequestedCabins(bookingDto.SelectedCabins, lockedCabins);

        var extraPrices = await GetCurrentExtraPrices(bookingDto);

        var booking = MapBooking(bookingDto, matchedCabins, extraPrices);

        await _bookingRepository.CreateBooking(booking);
        await _cabinRepository.ClearLockedCabins(bookingDto.CruiseDateId, _userInfo.Id);

        await _workflowService.StartBookingWorkflow(booking.Id);
    }

    private static IReadOnlyList<LockedCabin> MatchRequestedCabins(
        IReadOnlyList<BookingCabinSelection> requested,
        IReadOnlyList<LockedCabin> lockedCabins)
    {
        var pool = new List<LockedCabin>(lockedCabins);
        var matched = new List<LockedCabin>(requested.Count);

        foreach (var selection in requested)
        {
            var index = pool.FindIndex(lc =>
                lc.CabinId == selection.CabinId
                && lc.Occupants == selection.Occupants
                && lc.Price == selection.Price);

            if (index == -1)
            {
                throw new CabinNotAvailableException();
            }

            matched.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return matched;
    }

    private async Task<IReadOnlyDictionary<int, decimal>> GetCurrentExtraPrices(CreateBookingRequest bookingDto)
    {
        if (bookingDto.SelectedExtras.Count == 0)
        {
            return new Dictionary<int, decimal>();
        }

        var extras = await _extraRepository.GetExtrasAsync(bookingDto.CruiseDateId);
        return extras
            .Where(e => bookingDto.SelectedExtras.Contains(e.ExtraId))
            .ToDictionary(e => e.ExtraId, e => e.Price);
    }

    private Booking MapBooking(
        CreateBookingRequest bookingDto,
        IReadOnlyList<LockedCabin> lockedCabins,
        IReadOnlyDictionary<int, decimal> extraPrices)
    {
        return new Booking
        {
            CruiseDateId = bookingDto.CruiseDateId,
            UserId = _userInfo.Id,
            PaymentMethodId = bookingDto.PaymentMethodId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            Status = BookingStatus.Created,
            Cabins = [.. lockedCabins.Select(l => new BookingCabin
            {
                CabinId = l.CabinId,
                Price = l.Price,
                Occupants = l.Occupants
            })],
            Extras = [.. bookingDto.SelectedExtras.Select(e => new BookingExtra
            {
                ExtraId = e,
                Price = extraPrices.TryGetValue(e, out var ep) ? ep : 0m
            })]
        };
    }
}

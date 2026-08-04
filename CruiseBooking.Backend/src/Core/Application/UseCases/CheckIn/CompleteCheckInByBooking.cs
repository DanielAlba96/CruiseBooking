using Core.Application.Common.CQRS;
using Core.Application.Models;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Cierra el check-in de una reserva del usuario actual.</summary>
/// <param name="BookingId">Identificador de la reserva.</param>
public sealed record CompleteCheckInByBooking(int BookingId) : IRequest;

/// <summary>Atiende <see cref="CompleteCheckInByBooking"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="workflowService">Servicio de workflows, usado para notificar el check-in completado.</param>
/// <param name="userInfo">Datos del usuario autenticado en la petición actual.</param>
public sealed class CompleteCheckInByBookingHandler(
    IBookingRepository bookingRepository,
    IWorkflowService workflowService,
    UserInfo userInfo) : IRequestHandler<CompleteCheckInByBooking>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IWorkflowService _workflowService = workflowService;
    private readonly UserInfo _userInfo = userInfo;

    /// <inheritdoc />
    public async Task Handle(CompleteCheckInByBooking request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByOwner(_bookingRepository, request.BookingId, _userInfo.Id);

        await CheckInHelpers.Complete(_bookingRepository, _workflowService, booking);
    }
}

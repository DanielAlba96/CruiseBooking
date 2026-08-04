using Core.Application.Common.CQRS;
using Core.Application.UseCases.CheckIn.Common;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.CheckIn;

/// <summary>Cierra el check-in de la reserva identificada por el token enviado por correo.</summary>
/// <param name="Token">Token opaco de check-in.</param>
public sealed record CompleteCheckInByToken(string Token) : IRequest;

/// <summary>Atiende <see cref="CompleteCheckInByToken"/>.</summary>
/// <param name="bookingRepository">Repositorio de reservas.</param>
/// <param name="workflowService">Servicio de workflows, usado para notificar el check-in completado.</param>
public sealed class CompleteCheckInByTokenHandler(
    IBookingRepository bookingRepository,
    IWorkflowService workflowService) : IRequestHandler<CompleteCheckInByToken>
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IWorkflowService _workflowService = workflowService;

    /// <inheritdoc />
    public async Task Handle(CompleteCheckInByToken request, CancellationToken cancellationToken)
    {
        var booking = await CheckInHelpers.LoadByToken(_bookingRepository, request.Token);

        await CheckInHelpers.Complete(_bookingRepository, _workflowService, booking);
    }
}

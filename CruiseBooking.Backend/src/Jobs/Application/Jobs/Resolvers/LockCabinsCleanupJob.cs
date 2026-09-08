using Shared.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Shared.Domain.Models;

namespace Jobs.Application.Jobs.Resolvers;

/// <summary>
/// Borra los bloqueos de camarotes con los ids indicados
/// </summary>
public sealed class LockCabinsCleanupJob(ICabinRepository cabinRepository, ILogger<LockCabinsCleanupJob> logger) : IJob
{
    public const string NAME = "lock-cabins-cleanup";

    readonly ICabinRepository _cabinRepository = cabinRepository;
    readonly ILogger<LockCabinsCleanupJob> _logger = logger;

    /// <summary>
    /// Realiza el borrado de los camarotes bloqueados.
    /// </summary>
    /// <param name="data">Listado de camarotes bloqueados a borrar</param>
    /// <param name="cancellationToken">Token que cancela la operación contra la base de datos.</param>
    public async Task ExecuteAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        LockCabinsCleanupData? message;
        try
        {
            message = JsonSerializer.Deserialize<LockCabinsCleanupData>(payload.Span);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Dropping lock-cabins-cleanup request with an invalid body.");
            return;
        }

        if (message is null || message.LockedCabinsIds is null)
        {
            _logger.LogWarning("Dropping lock-cabins-cleanup request without cabin ids.");
            return;
        }

        await _cabinRepository.ClearLockedCabinsBulk(message.LockedCabinsIds, cancellationToken);
    }
}

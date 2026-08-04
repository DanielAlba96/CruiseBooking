namespace Shared.Domain.Models;

/// <summary>
/// Representa los datos necesarios para la limpieza de cabinas bloqueadas.
/// </summary>
public sealed record LockCabinsCleanupData(
    /// <summary>
    /// Identificadores de las cabinas bloqueadas que deben limpiarse.
    /// </summary>
    /// <value>
    /// Lista de solo lectura de identificadores de cabinas (valores enteros).
    /// </value>
    IReadOnlyList<int> LockedCabinsIds);

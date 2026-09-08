namespace Shared.Domain.Models;

/// <summary>
/// Representa los datos necesarios para la limpieza de camarotes bloqueados.
/// </summary>
public sealed record LockCabinsCleanupData(
    /// <summary>
    /// Identificadores de los camarotes bloqueados que deben limpiarse.
    /// </summary>
    /// <value>
    /// Lista de solo lectura de identificadores de camarotes (valores enteros).
    /// </value>
    IReadOnlyList<int> LockedCabinsIds);

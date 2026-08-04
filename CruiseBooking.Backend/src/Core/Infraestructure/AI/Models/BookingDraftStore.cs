using Microsoft.Extensions.Caching.Memory;

namespace Core.Infrastructure.AI.Models;

/// <summary>
/// Almacena el <see cref="BookingDraft"/> de cada sesión de chat en <see cref="IMemoryCache"/>,
/// de modo que las tools puedan compartir el borrador entre peticiones HTTP sucesivas.
/// </summary>
public sealed class BookingDraftStore(IMemoryCache cache)
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);
    private readonly IMemoryCache _cache = cache;

    private static string Key(Guid sessionId) => $"draft:{sessionId}";

    /// <summary>Obtiene el borrador de la sesión o <c>null</c> si no existe.</summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <returns>El borrador almacenado o <c>null</c> si ha expirado o no existe.</returns>
    internal BookingDraft? Get(Guid sessionId) =>
        _cache.TryGetValue(Key(sessionId), out BookingDraft? draft) ? draft : null;

    /// <summary>Reemplaza el borrador de la sesión.</summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    /// <param name="draft">El nuevo borrador a almacenar.</param>
    internal void Set(Guid sessionId, BookingDraft draft) =>
        _cache.Set(Key(sessionId), draft, new MemoryCacheEntryOptions { SlidingExpiration = Expiration });

    /// <summary>Elimina el borrador de la sesión.</summary>
    /// <param name="sessionId">Identificador único de la sesión de chat.</param>
    internal void Clear(Guid sessionId) => _cache.Remove(Key(sessionId));
}

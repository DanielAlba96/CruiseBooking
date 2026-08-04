namespace CruiseBooking.Services;

/// <summary>
/// Gestiona los tokens de autenticación en caché para el circuito Blazor actual.
/// </summary>
public class AuthTokenStateService
{
    /// <summary>
    /// Obtiene el token de acceso actual.
    /// </summary>
    public string AccessToken { get; private set; } = string.Empty;

    /// <summary>
    /// Obtiene el token de actualización actual.
    /// </summary>
    public string RefreshToken { get; private set; } = string.Empty;

    /// <summary>
    /// Obtiene la hora de vencimiento del token de acceso.
    /// </summary>
    public string ExpiresAt { get; private set; } = string.Empty;

    /// <summary>
    /// Actualiza los tokens en caché.
    /// </summary>
    /// <param name="accessToken">El valor del token de acceso.</param>
    /// <param name="refreshToken">El valor del token de actualización.</param>
    /// <param name="expiresAt">La hora de vencimiento del token de acceso.</param>
    public void SetTokens(string? accessToken, string? refreshToken, string? expiresAt)
    {
        AccessToken = accessToken ?? string.Empty;
        RefreshToken = refreshToken ?? string.Empty;
        ExpiresAt = expiresAt ?? string.Empty;
    }

    /// <summary>
    /// Borra todos los tokens en caché.
    /// </summary>
    public void Clear()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        ExpiresAt = string.Empty;
    }
}

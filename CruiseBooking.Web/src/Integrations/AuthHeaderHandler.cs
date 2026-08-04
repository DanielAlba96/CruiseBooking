using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using CruiseBooking.Services;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CruiseBooking.Integrations;

/// <summary>
/// Maneja la actualización automática del token y la vinculación de encabezados de autorización a las solicitudes HTTP salientes.
/// </summary>
public class AuthHeaderHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
    AuthTokenStateService tokenState) : DelegatingHandler
{
    static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(1);

    readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    readonly IOptionsMonitor<OpenIdConnectOptions> _oidcOptions = oidcOptions;
    readonly AuthTokenStateService _tokenState = tokenState ?? new AuthTokenStateService();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal is null || authResult.Properties is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var accessToken = GetTokenValue(_tokenState.AccessToken, authResult.Properties.GetTokenValue("access_token"));
        var refreshToken = GetTokenValue(_tokenState.RefreshToken, authResult.Properties.GetTokenValue("refresh_token"));
        var expiresAt = GetTokenValue(_tokenState.ExpiresAt, authResult.Properties.GetTokenValue("expires_at"));

        if (ShouldRefreshAccessToken(expiresAt))
        {
            var refreshResult = await TryRefreshAccessTokenAsync(refreshToken, cancellationToken);
            if (refreshResult is null)
            {
                _tokenState.Clear();
                await TrySignOutAndRedirectToLoginAsync(httpContext);

                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    RequestMessage = request
                };
            }

            accessToken = refreshResult.AccessToken;
            refreshToken = refreshResult.RefreshToken;
            expiresAt = refreshResult.ExpiresAt;

            _tokenState.SetTokens(accessToken, refreshToken, expiresAt);

            if (!httpContext.Response.HasStarted)
            {
                authResult.Properties.UpdateTokenValue("access_token", accessToken);
                authResult.Properties.UpdateTokenValue("refresh_token", refreshToken);
                authResult.Properties.UpdateTokenValue("expires_at", expiresAt);

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal,
                    authResult.Properties);
            }
        }
        else
        {
            _tokenState.SetTokens(accessToken, refreshToken, expiresAt);
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    async Task<RefreshTokenResult?> TryRefreshAccessTokenAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var options = _oidcOptions.Get(OpenIdConnectDefaults.AuthenticationScheme);
        var configurationManager = options.ConfigurationManager;
        if (configurationManager is null)
        {
            return null;
        }

        var oidcConfiguration = await configurationManager.GetConfigurationAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(oidcConfiguration.TokenEndpoint))
        {
            return null;
        }

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, oidcConfiguration.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = options.ClientId ?? string.Empty,
                ["client_secret"] = options.ClientSecret ?? string.Empty,
                ["refresh_token"] = refreshToken
            })
        };

        using var tokenResponse = await base.SendAsync(tokenRequest, cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            return null;
        }

        await using var responseStream = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<TokenRefreshResponse>(responseStream, cancellationToken: cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken) || payload.ExpiresIn <= 0)
        {
            return null;
        }

        return new RefreshTokenResult(
            payload.AccessToken,
            string.IsNullOrWhiteSpace(payload.RefreshToken) ? refreshToken : payload.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn).ToString("o", CultureInfo.InvariantCulture));
    }

    static bool ShouldRefreshAccessToken(string? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(expiresAt))
        {
            return false;
        }

        return !DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAtValue)
            || expiresAtValue <= DateTimeOffset.UtcNow.Add(RefreshThreshold);
    }

    static async Task TrySignOutAndRedirectToLoginAsync(HttpContext httpContext)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var currentPath = $"{httpContext.Request.PathBase}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        var returnUrl = string.IsNullOrWhiteSpace(currentPath) ? "/" : currentPath;
        var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

        httpContext.Response.Redirect(loginUrl);
    }

    static string GetTokenValue(string currentValue, string? persistedValue)
        => !string.IsNullOrWhiteSpace(currentValue) ? currentValue : persistedValue ?? string.Empty;

    sealed record RefreshTokenResult(string AccessToken, string RefreshToken, string ExpiresAt);

    sealed class TokenRefreshResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}

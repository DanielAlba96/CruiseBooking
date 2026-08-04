using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace CruiseBooking.Endpoints;

/// <summary>
/// Configura los puntos finales relacionados con autenticación para la aplicación.
/// </summary>
public static class AuthenticationEndpoints
{
    /// <summary>
    /// Asigna los puntos finales de autenticación (login, devolución de llamada de login, logout) a la aplicación.
    /// </summary>
    /// <param name="app">La aplicación web a configurar.</param>
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapGet("/login", async (HttpContext http, string? returnUrl) =>
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/login-callback"
            };

            props.Items["returnUrl"] = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";

            await http.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, props);
        }).AllowAnonymous();

        app.MapGet("/login-callback", async (HttpContext http, IBookingApiService api) =>
        {
            var result = await http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal is null)
            {
                return Results.Unauthorized();
            }

            var userGuid = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userGuid is null)
            {
                return Results.Unauthorized();
            }

            await api.CreateOrUpdateUserAsync(
                new CreateOrUpdateUserRequest
                (
                    result.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                    result.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                    result.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                    result.Principal.FindFirstValue("phone_number") ?? string.Empty,
                    result.Principal.FindFirstValue("address") ?? string.Empty
                ),
                http.RequestAborted);

            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal, result.Properties);

            var returnUrl = "/";
            if (result.Properties?.Items != null
                && result.Properties.Items.TryGetValue("returnUrl", out var storedUrl)
                && !string.IsNullOrWhiteSpace(storedUrl))
            {
                returnUrl = storedUrl;
            }

            // Garantizamos que se redireccione a una direccion local para evitar ataques de open redirect
            return Results.LocalRedirect(returnUrl);
        }).AllowAnonymous();

        app.MapGet("/logout", () =>
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            return Results.SignOut(props,
            [
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            ]);
        });
    }
}
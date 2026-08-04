using Core.Application.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Repositories;
using Shared.Persistence;
using System.Security.Claims;

namespace Core.Api;

/// <summary>
/// Extensiones de dependencia personalizadas para la configuración de autenticación y validación de usuarios.
/// </summary>
public static class CustomExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configura la autenticación JWT con Keycloak en la aplicación.
        /// </summary>
        /// <param name="authority">URL de la autoridad de Keycloak.</param>
        /// <param name="audience">Identificador de audiencia del token JWT.</param>
        /// <param name="isProduction">Indica si se ejecuta en ambiente de producción.</param>
        public void AddKeycloakAuthentication(string? authority, string? audience, bool isProduction)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                    options.RequireHttpsMetadata = isProduction;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Error de autenticación: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? context.Principal?.FindFirst("sub")?.Value;

                            if (!Guid.TryParse(userIdClaim, out var userGuid))
                            {
                                context.Fail("Token inválido: claim de identidad ausente o malformado.");
                                return;
                            }

                            var userRepository = context.HttpContext.RequestServices
                                .GetRequiredService<IUserRepository>();

                            var currentUser = context.HttpContext.RequestServices
                                .GetRequiredService<UserInfo>();

                            currentUser.KeycloakGuid = userGuid;

                            var dbUser = await userRepository.GetUserByKeycloakGuid(userGuid);

                            if (dbUser is not null)
                            {
                                currentUser.Id = dbUser.Id;
                                currentUser.Name = dbUser.Name;
                                currentUser.Surname = dbUser.Surname;
                                currentUser.Email = dbUser.Email;
                                currentUser.CustomerId = dbUser.CustomerId;
                            }
                        }
                    };
                });
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Aplica las migraciones pendientes de Entity Framework Core a la base de datos.
        /// </summary>
        public void ApplyMigrations()
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CruisesDbContext>();
            db.Database.Migrate();
        }

        /// <summary>
        /// Configura un middleware que valida la información del usuario autenticado.
        /// </summary>
        public void UseUserInfoValidation()
        {
            app.Use(async (context, next) =>
            {
                bool isUserCreationEndpoint = context.Request.Method == HttpMethods.Post
                    && string.Equals(context.Request.Path.Value, "/api/user/current", StringComparison.OrdinalIgnoreCase);

                if (context.User.Identity?.IsAuthenticated == true && !isUserCreationEndpoint)
                {
                    var userInfo = context.RequestServices.GetRequiredService<UserInfo>();

                    if (userInfo.Id == 0)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Title = "Usuario no encontrado",
                            Status = StatusCodes.Status401Unauthorized,
                            Detail = "El token es válido pero el usuario no esta correctamente registrado en la aplicación"
                        });
                        return;
                    }
                }

                await next(context);
            });
        }
    }
}

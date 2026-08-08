using Core.Agents.Chat;
using Core.Agents.Common.Middleware;
using Core.Agents.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace Core.Agents;

/// <summary>
/// Registra los servicios propios de los agentes IA en el contenedor de inyección de dependencias
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa de de los agentes IA .
    /// </summary>
    /// <param name="builder">El builder de la aplicación host.</param>
    /// <returns>El mismo builder, para poder encadenar llamadas.</returns>
    public static IHostApplicationBuilder AddCoreAgentServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IChatManager, ChatManager>();

        var ollamaOptions = builder.Configuration.GetSection(OllamaSettings.SectionName).Get<OllamaSettings>()
            ?? new OllamaSettings();

        builder.Services.AddSingleton<IChatClient>(_ => new OllamaApiClient(new Uri(ollamaOptions.BaseUrl), ollamaOptions.Model));
        builder.Services.AddSingleton<FunctionLoggingMiddleware>();

        return builder;
    }
}

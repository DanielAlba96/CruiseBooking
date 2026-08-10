using Core.Agents.Chat;
using Core.Agents.Chat.Approvals;
using Core.Agents.Common;
using Core.Agents.Common.Middleware;
using Core.Agents.Orchestration;
using Core.Agents.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;
using OpenAI;
using System.ClientModel;

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
        var openAIOptions = builder.Configuration.GetSection(OpenAISettings.SectionName).Get<OpenAISettings>()
            ?? new OpenAISettings();

        if (openAIOptions.Enabled)
        {
            builder.Services.AddSingleton<IChatClient>(_ =>
            {
                var client = new OpenAIClient(
                    new ApiKeyCredential(openAIOptions.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(openAIOptions.BaseUrl) });

                return client.GetChatClient(openAIOptions.Model).AsIChatClient();
            });
        }
        else
        {
            var ollamaOptions = builder.Configuration.GetSection(OllamaSettings.SectionName).Get<OllamaSettings>()
                ?? new OllamaSettings();

            builder.Services.AddSingleton<IChatClient>(_ => new OllamaApiClient(new Uri(ollamaOptions.BaseUrl), ollamaOptions.Model));
        }

        builder.Services.AddSingleton<FunctionLoggingMiddleware>();
        builder.Services.AddScoped<IAgentFactory, AgentFactory>();
        builder.Services.AddScoped<IWorkflowFactory, WorkflowFactory>();
        builder.Services.AddScoped<IChatManager, WorkflowChatManager>();

        builder.Services.AddKeyedScoped<IToolApprovalHandler, ConfirmBookingApprovalHandler>(ApprovalToolNames.ConfirmBooking);
        builder.Services.AddKeyedScoped<IToolApprovalHandler, PayBookingApprovalHandler>(ApprovalToolNames.PayBooking);
        builder.Services.AddKeyedScoped<IToolApprovalHandler, CancelBookingApprovalHandler>(ApprovalToolNames.CancelBooking);

        return builder;
    }
}

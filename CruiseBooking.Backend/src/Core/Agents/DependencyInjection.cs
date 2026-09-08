using Core.Agents.Chat;
using Core.Agents.Chat.Approvals;
using Core.Agents.Chat.Compaction;
using Core.Agents.Chat.Orchestration;
using Core.Agents.Common;
using Core.Agents.Common.Middleware;
using Core.Agents.Settings;
using Microsoft.Agents.AI.Compaction;
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
    /// Registra los servicios de la capa de los agentes IA .
    /// </summary>
    /// <param name="builder">El builder de la aplicación host.</param>
    /// <returns>El mismo builder, para poder encadenar llamadas.</returns>
    public static IHostApplicationBuilder AddCoreAgentServices(this IHostApplicationBuilder builder)
    {
        var openAIOptions = builder.Configuration.GetSection(OpenAISettings.SectionName).Get<OpenAISettings>()
            ?? new OpenAISettings();

        IChatClient chatClient;
        if (openAIOptions.Enabled)
        {
            chatClient = new OpenAIClient(
                    new ApiKeyCredential(openAIOptions.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(openAIOptions.BaseUrl) })
                .GetChatClient(openAIOptions.Model).AsIChatClient();
        }
        else
        {
            var ollamaOptions = builder.Configuration.GetSection(OllamaSettings.SectionName).Get<OllamaSettings>()
                ?? new OllamaSettings();

            chatClient = new OllamaApiClient(new Uri(ollamaOptions.BaseUrl), ollamaOptions.Model);
        }

        builder.Services.AddSingleton<IChatClient>(chatClient);

        builder.Services.AddSingleton<FunctionLoggingMiddleware>();
        builder.Services.AddScoped<IAgentFactory, AgentFactory>();
        builder.Services.AddScoped<IWorkflowFactory, WorkflowFactory>();
        builder.Services.AddScoped<IChatManager, WorkflowChatManager>();

        builder.Services.AddKeyedScoped<IToolApprovalHandler, ConfirmBookingApprovalHandler>(BookingToolNames.ConfirmBooking);
        builder.Services.AddKeyedScoped<IToolApprovalHandler, PayBookingApprovalHandler>(PostSalesToolNames.PayBooking);
        builder.Services.AddKeyedScoped<IToolApprovalHandler, CancelBookingApprovalHandler>(PostSalesToolNames.CancelBooking);

#pragma warning disable MAAI001 // El framework de compaction esta actualmente en fase experimental
        builder.Services.AddSingleton<IChatReducer>(_ => new PipelineCompactionStrategy(
            new BookingContextCompactionStrategy(),
            new TruncationCompactionStrategy(CompactionTriggers.TokensExceed(8_000), 16, CompactionTriggers.TokensExceed(2_000))
        ).AsChatReducer());
#pragma warning restore MAAI001 // El framework de compaction esta actualmente en fase experimental

        return builder;
    }
}

using Core.Infrastructure.AI;
using Core.Infrastructure.AI.Models;
using Core.Infrastructure.Options;
using Core.Infrastructure.Services;
using Core.Infrastructure.Workflows;
using Core.Infrastructure.Workflows.Activities;
using Dapr.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;
using Shared.Domain.Services;
using Shared.Persistence;
using Stripe;

namespace Core.Infrastructure;

/// <summary>
/// Registra los servicios propios de la capa de Infraestructura en el contenedor de inyección de dependencias: acceso a datos, clientes externos y el cableado de los workflows en segundo plano.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la capa de Infraestructura.
    /// </summary>
    /// <param name="builder">El builder de la aplicación host.</param>
    /// <returns>El mismo builder, para poder encadenar llamadas.</returns>
    public static IHostApplicationBuilder AddCoreInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var stripeOptions = builder.Configuration.GetSection(StripeOptions.SectionName).Get<StripeOptions>()
            ?? new StripeOptions();

        if (stripeOptions.Enabled)
        {
            StripeConfiguration.ApiKey = stripeOptions.SecretKey;
            builder.Services.AddScoped<IPaymentService, StripePaymentService>();           
        }
        else
        {
            builder.Services.AddScoped<IPaymentService, FakePaymentService>();
        }

        builder.Services.AddDaprClient();
        builder.AddDataServices();

        builder.Services.AddDaprWorkflow(options =>
        {
            options.RegisterWorkflow<BookingWorkflow>();
            options.RegisterActivity<GetBookingActivity>();
            options.RegisterActivity<CompletePaymentActivity>();
            options.RegisterActivity<CancelUnpaidBookingActivity>();
            options.RegisterActivity<CompleteCheckInActivity>();
            options.RegisterActivity<SendBookingCanceledEmailActivity>();
            options.RegisterActivity<SendPaymentErrorEmailActivity>();
            options.RegisterActivity<SendPaymentDeadlineEmailActivity>();
            options.RegisterActivity<EnqueueInvoiceActivity>();
            options.RegisterActivity<SendCheckInEmailActivity>();
        });

        builder.Services.Configure<CheckInOptions>(builder.Configuration.GetSection(CheckInOptions.SectionName));
        builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
        builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));

        builder.Services.AddScoped<IWorkflowService, DaprWorkflowService>();
        builder.Services.AddScoped<BookingDraftStore>();
        builder.Services.AddScoped<IChatManager, OllamaChatManager>();
        builder.Services.AddScoped<IJobService, DaprJobService>();

        var ollamaOptions = builder.Configuration.GetSection(OllamaOptions.SectionName).Get<OllamaOptions>()
            ?? new OllamaOptions();

        builder.Services.AddSingleton<IOllamaApiClient>(_ => new OllamaApiClient(new Uri(ollamaOptions.BaseUrl), ollamaOptions.Model));

        return builder;
    }
}

using Core.Infrastructure.Settings;
using Core.Infrastructure.Services;
using Core.Infrastructure.Workflows;
using Core.Infrastructure.Workflows.Activities;
using Dapr.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        var stripeOptions = builder.Configuration.GetSection(StripeSettings.SectionName).Get<StripeSettings>()
            ?? new StripeSettings();

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

        builder.Services.Configure<CheckInSettings>(builder.Configuration.GetSection(CheckInSettings.SectionName));
        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(StripeSettings.SectionName));

        builder.Services.AddScoped<IWorkflowService, DaprWorkflowService>();
        builder.Services.AddScoped<ICacheService, DaprCacheService>();
        builder.Services.AddScoped<IJobService, DaprJobService>();

        return builder;
    }
}

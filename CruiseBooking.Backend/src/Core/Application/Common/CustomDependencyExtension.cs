using Core.Application.Common.CQRS;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Application.Common;

internal static class CustomDependencyExtension
{
    extension(IServiceCollection services)
    {
        public void AddCustomMediator()
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddScoped<IMediator, Mediator>();

            services.Scan(scan => scan.FromAssemblies(assembly)
             .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)), publicOnly: false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
        }
    }
}

using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithEndpointProxySupport(false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var cruisesdb = postgres.AddDatabase("CruisesDb");
var daprStateDb = postgres.AddDatabase("daprstate");

var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var rabbit = builder.AddRabbitMQ("rabbitmq", username, password, 5672)
    .WithEndpointProxySupport(false)
    .WithManagementPlugin();

var mailpit = builder.AddMailPit("mailpit");

var daprConfigCore = new DaprSidecarOptions
{
    AppId = "core",
    DaprGrpcPort = 50001,
    DaprHttpPort = 3500,
    MetricsPort = 9090,
    ResourcesPaths = ["../../src/Core/Api/ResourcesLocal"]
};

var daprConfigJobs = new DaprSidecarOptions
{
    AppId = "jobs",
    DaprGrpcPort = 50002,
    DaprHttpPort = 3501,
    MetricsPort = 9091,
    ResourcesPaths = ["../../src/Jobs/Api/ResourcesLocal"]
};

builder.AddProject<Projects.Core_Api>("core-api")
    .WaitFor(cruisesdb)
    .WaitFor(daprStateDb)
    .WaitFor(rabbit)
    .WithReference(cruisesdb)
    .WithDaprSidecar(sidecarBuilder =>
    {
        sidecarBuilder
            .WithOptions(daprConfigCore)
            .WithAnnotation(
                new EnvironmentCallbackAnnotation(context =>
                {
                    context.EnvironmentVariables["RABBITMQ_CONNECTION_STRING"] = rabbit.Resource.ConnectionStringExpression;
                    context.EnvironmentVariables["POSTGRES_CONNECTION_STRING"] = ReferenceExpression.Create(
                        $"host={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)} port={postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)} user=postgres password={postgres.Resource.PasswordParameter} dbname=daprstate sslmode=disable");

                    return Task.CompletedTask;
                }));
    });

builder.AddProject<Projects.Jobs_Api>("jobs-api")
    .WaitFor(cruisesdb)
    .WaitFor(rabbit)
    .WaitFor(mailpit)
    .WithReference(cruisesdb)
    .WithReference(mailpit)
    .WithDaprSidecar(sidecarBuilder =>
     {
         sidecarBuilder
             .WithOptions(daprConfigJobs)
             .WithAnnotation(
                 new EnvironmentCallbackAnnotation(context =>
                 {
                     context.EnvironmentVariables["RABBITMQ_CONNECTION_STRING"] = rabbit.Resource.ConnectionStringExpression;
                     return Task.CompletedTask;
                 }));
     });

builder.AddProject<Projects.Cruises_Api>("cruise-data-api")
    .WaitFor(cruisesdb)
    .WithReference(cruisesdb);

builder.Build().Run();

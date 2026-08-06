# CLAUDE.md

This application is the backend for a cruise booking system.

## Tech Stack

- **.NET 10** — all projects target `net10.0`
- **ASP.NET Core Minimal APIs + Carter** — REST endpoints defined in `src/Core/Api/Endpoints/`
- **HotChocolate 15** — GraphQL layer at `/graphql`, hosted by `src/Cruises/Api/`; query types in `src/Cruises/Api/GraphQL/`
- **Entity Framework Core 10 + Npgsql** — PostgreSQL via `IDbContextFactory<CruisesDbContext>`; DbContext, entity configurations, repositories, and migrations all live in `src/Shared/Persistence/`
- **Dapr** — workflow orchestration (`src/Core/Infraestructure/Workflows/`), pub/sub over RabbitMQ, and background jobs (`src/Jobs/`)
- **RabbitMQ** — Dapr pub/sub transport between Core and Jobs (`jobs-pub-sub` component, topic `jobs`)
- **Keycloak** — JWT auth (`JwtBearerDefaults`); authority/audience configured in `appsettings.json` under `Keycloak:`. Not orchestrated by Aspire — run it manually (see `aspire/AppHost/keycloak/start-keycloak.txt`, realm export in the same folder)
- **Stripe** — payments, payment methods, refunds and tax rates via `StripePaymentService` (`Stripe.net`)
- **Ollama** — AI chat assistant (`OllamaChatManager` + tools in `src/Core/Infraestructure/AI/`)
- **MailKit** — email via `SmtpEmailService` in `src/Jobs/Infrastructure/`; dev SMTP trapped by Mailpit container
- **PDFsharp-MigraDoc** — invoice PDF generation in `src/Jobs/Application/Services/Impl/InvoiceService.cs`
- **.NET Aspire 13** — `aspire/AppHost/` orchestrates containers and the three API hosts

## Developer Commands

```bash
# Run everything via Aspire (starts all containers + API hosts)
dotnet run --project aspire/AppHost

# Run an API host standalone (requires external services)
dotnet run --project src/Core/Api      # REST endpoints
dotnet run --project src/Cruises/Api   # GraphQL
dotnet run --project src/Jobs/Api      # Dapr jobs handler

# Add EF migration (run from repo root; migrations live in Shared.Persistence)
dotnet ef migrations add <Name> --project src/Shared/Persistence --startup-project src/Core/Api

# Apply pending migrations manually
dotnet ef database update --project src/Shared/Persistence --startup-project src/Core/Api

# Build entire solution
dotnet build CruiseBooking.Backend.slnx

```

Migrations are also applied automatically at startup via `app.ApplyMigrations()` in `src/Core/Api/Program.cs`.

Keycloak is not started by Aspire; launch the container manually with the commands in `aspire/AppHost/keycloak/start-keycloak.txt` before using authenticated endpoints.

## Architecture

Clean Architecture split into three bounded hosts (`Core`, `Cruises`, `Jobs`) over a shared kernel (`Shared`):

| Project | Role |
|---|---|
| `src/Shared/Domain/` | Entities, enums, repository interfaces (`IBookingRepository`, `IUserRepository`, …), service interfaces (`IPaymentService`, `IEmailService`, `IJobService`, `IWorkflowService`, `IChatManager`), shared models, `ControlledException` base |
| `src/Shared/Persistence/` | EF Core `CruisesDbContext`, entity `Configurators/`, repository implementations, migrations, `DbSeeder` |
| `src/Core/Application/` | Use cases (request + response + handler), custom CQRS mediator (`Common/CQRS/`), application exceptions, DTOs/models |
| `src/Core/Infraestructure/` | Dapr workflows/activities, `StripePaymentService`, `DaprJobService`, `DaprWorkflowService`, `Settings/` |
| `src/Core/Api/` | Minimal API + Carter REST endpoints, Keycloak auth, OpenAPI/Scalar, `GlobalExceptionHandler`, Dapr components in `ResourcesLocal/` |
| `src/Core/Agents/` | AI Agents with their tools and configurations |
| `src/Cruises/Api/` | HotChocolate GraphQL host (read-only); depends only on `Shared.Persistence` |
| `src/Jobs/Application/` | Jobs as use cases (`IJob` keyed implementations + `IJobResolver`), `InvoiceService` |
| `src/Jobs/Infrastructure/` | `SmtpEmailService`, `Options/` |
| `src/Jobs/Api/` | Dapr Jobs host: scheduled-job handler + `/jobs` scheduling endpoint; Dapr components in `ResourcesLocal/` |
| `aspire/AppHost/` | Aspire orchestration |
| `aspire/ServiceDefaults/` | OpenTelemetry, health checks, service discovery, HTTP resilience (`AddServiceDefaults()` / `MapDefaultEndpoints()`) |

## Key Conventions

- **CQRS use cases**: There is no service layer in `Core`. Each use case lives in a single file under `Core/Application/UseCases/<Area>/` containing the request record, the response record, and the handler. Endpoints only call `IMediator.Send(...)`. The mediator is hand-rolled (`Common/CQRS/`) — `IRequest` and `IRequest<TResponse>` are disjoint hierarchies, no `Unit`, no pipeline behaviors. Handlers are auto-registered with Scrutor in `AddCustomMediator()`.
- **Handlers use repositories directly**; shared logic between handlers goes into `internal static` helpers under `UseCases/<Area>/Common/`.
- **Error handling**: Throw an exception derived from `ControlledException` (it carries its own `StatusCode`). `GlobalExceptionHandler` maps it to `ProblemDetails`; anything else becomes a generic 500. Never return error results manually from an endpoint.
- **Authentication flow**: On every validated JWT, `OnTokenValidated` in `src/Core/Api/Common/CustomDependencyExtensions.cs` resolves the Keycloak GUID from the `sub` claim, looks up the local `User` record, and populates a scoped `UserInfo` service (including `CustomerId`, the Stripe customer). `UseUserInfoValidation()` then rejects authenticated tokens with no local user, except on `POST /api/users/current`. Endpoints and handlers inject `UserInfo` — never read claims manually.
- **Payments**: `IPaymentService` is wired to `StripePaymentService`. Tax rates come from Stripe based on the customer's address (`GetTaxRate` use case) — they are no longer read from configuration.
- **Pricing**: Bookings store frozen net lines, tax rate, and gross charge amount; payment flows read those values and never recalculate them.
- **Background work**: Core publishes a `ScheduleJobRequest` to the `jobs-pub-sub` topic (`DaprJobService`); `Jobs.Api` subscribes on `/jobs`, schedules a Dapr job, and dispatches the trigger to the keyed `IJob` resolved by `IJobResolver` (`send-email`, `generate-invoice`, `lock-cabins-cleanup`).
- **Workflows**: `BookingWorkflow` plus its activities are registered in `AddCoreInfrastructureServices()`. Exceptions do not cross the Dapr boundary as objects; `IsCausedBy<T>` is an exact-type match and there is no retry predicate.
- **DI wiring**: Each project registers itself — `AddDataServices()` (`Shared.Persistence`), `AddApplicationServices()` (`Core.Application`), `AddCoreInfrastructureServices()` (`Core.Infrastructure`), `AddJobsApplicationServices()` / `AddJobsInfrastructureServices()` (`Jobs`), `AddCruiseGraphQL()` (`Cruises.Api`).
- **GraphQL**: Read-only queries only — all mutations go through the Minimal API endpoints in `Core.Api`.
- **EF context usage**: Repositories create contexts via `IDbContextFactory` (`using var context = ...`), not via a DI-injected context, to be safe under concurrent GraphQL resolvers. Repository methods returning `IQueryable` are for GraphQL only; add async materializing methods for handlers and tools.
- **Resources**: User-facing messages come from `ErrorMessages.resx` in the project that throws (`Core.Application`, `Shared.Persistence`, `Jobs.Infrastructure`).
- **Options**: Options pattern classes live in each project's `Settings/` folder.

## Configuration

`src/Core/Api/appsettings.json`:
- `ConnectionStrings:CruisesDb` — PostgreSQL connection string (injected by Aspire; use user secrets standalone)
- `Keycloak:Authority` / `Keycloak:Audience` — realm URL and client ID
- `CheckIn:FrontendBaseUrl` — frontend base URL used to build the check-in link
- `Ollama:BaseUrl` / `Ollama:Model` — AI chat endpoint and model
- `Stripe:SecretKey` / `Stripe:PublishableKey` — Stripe API keys (user secrets)
- `LockedCabins:ExpirationMinutes` — how long a cabin lock survives before cleanup is scheduled

`src/Jobs/Api/appsettings.json`:
- `ConnectionStrings:CruisesDb`
- `Email:FromAddress` / `Email:FromName` — sender identity for MailKit
- `LockCabinsCleanup:IntervalMinutes` / `MaxAgeMinutes`

Aspire injects connection strings automatically when running via `AppHost`; `username`, and `password` (RabbitMQ) are Aspire secret parameters.

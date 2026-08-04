# CLAUDE.md

This application is the frontend for a cruise booking system.

## Stack

- **Framework**: Blazor Server (.NET 10, Interactive Server render mode)
- **UI**: MudBlazor 8
- **GraphQL client**: StrawberryShake (code-generated, `dotnet graphql generate`)
- **REST client**: Refit (`IBookingApi`, internal) behind the `IBookingApiService` facade (ApiResult pattern)
- **Object mapping**: Manual mapping
- **Validation**: FluentValidation
- **Auth**: OIDC (Keycloak) + Cookie auth via `Microsoft.AspNetCore.Authentication.OpenIdConnect`

## Developer Commands

```bash
# Build
dotnet build

# Run
dotnet run --project src/CruiseBooking.csproj

# Regenerate GraphQL client after adding/modifying .graphql query files (must be done inside src/GraphQL folder)
dotnet graphql generate
```

## External Services (must be running locally)

| Service | URL | Config key |
|---|---|---|
| DataApi (GraphQL reads) | `DataApi:Url` |
| BookingApi (REST business logic) | `BookingApi:Url` |
| Keycloak OIDC realm | `Keycloak:Authority` |

- GraphQL codegen (`dotnet graphql generate`) reads its own DataApi URL from `src/GraphQL/.graphqlrc.json`, separate from the runtime `DataApi:Url` above — update both if the DataApi url changes.
- Backend is split into two independent APIs: one serves GraphQL reads, the other owns REST business logic (bookings, payments, chat).

## Project Structure

```
src/
  Components/
    Pages/          # Blazor pages (.razor + .razor.cs code-behind)
    Layout/         # MainLayout, ReconnectModal
    Shared/         # Cross-page components: PaymentMethodSelector, CardBrandIcon
  Endpoints/        # Minimal API endpoints (auth flow: /login, /login-callback, /logout)
  GraphQL/          # schema.graphql + query files (Queries/); generated client at obj/Debug/{tfm}/berry/CruisesClient.Client.cs (source-generated build output, not checked in), registered via AddCruisesClient() in Program.cs
  Integrations/     # IBookingApi, IChatApi (Refit interfaces), AuthHeaderHandler (token refresh DelegatingHandler)
  Mappings/         # Manual mapper: GraphQL → domain DTOs
  Services/         # BookingStateService, AuthTokenStateService, StripeOptions, Dtos/
  Services/Api/     # IBookingApiService + ApiResult: facade over IBookingApi; Api/CheckIn/ = check-in gateways
  Vms/              # ViewModel records used across pages
```

## Booking Wizard Flow

Pages form a linear wizard; `BookingStateService` carries state across steps:

```
/cruise-search  →  /book-cruise?cruiseId=&cruiseDateId=  →  /booking-summary
```

Each page validates prior steps on `OnInitializedAsync` and redirects back if incomplete. After `BookingSummary` succeeds, it calls `_bookingContainer.Clear()` and navigates to `/`.

`BookingSummary.CompleteBooking` resolves the payment method via `PaymentMethodSelector.ResolvePaymentMethodIdAsync` (confirms a Stripe SetupIntent client-side) and passes it to `SaveBookingAsync`, which persists the booking. No charge happens at this point — the resolved payment method is stored for automatic charging later.

## Page Routes

| Route | Page | Purpose |
|---|---|---|
| `/` | `Home` | Featured cruises |
| `/cruise-search` | `SearchCruises` | Filter + cursor-paginated cruise list |
| `/book-cruise` | `BookCruise` | Select cabins & extras |
| `/booking-summary` | `BookingSummary` | Payment + confirm booking |
| `/my-bookings` | `MyBookings` | Booking history + cancellation |
| `/my-payment-methods` | `MyPaymentMethods` | Manage saved payment methods |
| `/pay-booking/{BookingId:int}` | `PayBooking` | Manually pay a booking |
| `/chat` | `Chat` | AI booking assistant chat |
| `/check-in` | `CheckIn` | Check-in entry via public token link |
| `/check-in/booking/{BookingId:int}` | `CheckInBooking` | Check-in entry for an authenticated booking |
| `/booking-passengers` | `PassengersForm` | Check-in passenger details (public token link) |
| `/booking-passengers/booking/{BookingId:int}` | `PassengersFormBooking` | Check-in passenger details (authenticated booking) |
| `/not-found` | `NotFound` | 404 fallback |
| `/Error` | `Error` | Unhandled error page |

`/booking-passengers*` is part of the **check-in** feature, not the booking wizard — both routes build an `ICheckInGateway` (`TokenCheckInGateway` / `BookingCheckInGateway`) and render the shared `PassengersFormView` component (no `@page` of its own). `CheckInView` is the equivalent no-route child component for `CheckIn`/`CheckInBooking`.

`SearchCruises` uses cursor-based pagination (StrawberryShake connection model): `_lastCursor` advances on "load more"; changing sort resets cursor and clears the list.

## Key Conventions

- **UI language**: App is in Spanish — all user-facing strings, snackbar messages, and validation errors are in Spanish.
- **Razor pages**: All pages split markup (`.razor`) from logic (`.razor.cs`). No inline `@code` blocks except `RedirectToLogin.razor`.
- **Data access split**: GraphQL (StrawberryShake) for reads; `IBookingApiService` (facade over the Refit `IBookingApi`) for mutations and user/booking management. New reads should add a `.graphql` query under `src/GraphQL/Queries/` and regenerate.
- **REST calls**: screens never use Refit directly. They inject `IBookingApiService`, which returns `ApiResult`/`ApiResult<T>` and **never throws**. On failure, `result.ErrorMessage` is always populated (`ProblemDetails.Detail` from the backend, or a generic message if there's no body) — callers display it as-is, with no fallback of their own (`result.ErrorMessage!`). `result.IsUnauthorized` covers the 401 case. All error translation lives in `BookingApiService.ExecuteAsync`.
- **UI error convention**: button-triggered actions (save, cancel, delete, complete) show an error snackbar with `result.ErrorMessage!`. Page loads (`OnInitializedAsync`/`OnParametersSetAsync`) that block rendering show a persistent `MudAlert` + "Retry" button that re-invokes the load (see `BookCruise`, `BookingSummary` (taxes), `MyBookings`, `CheckInView`). Deliberate exceptions: `PaymentMethodSelector.LoadPaymentMethodsAsync` (component in `Components/Shared/`, used by `BookingSummary`) degrades to a new card without blocking, and `PassengersFormView.LoadCabinAsync` navigates away from the page instead of retrying in place.
- **GraphQL codegen**: `CruiseBooking.csproj` has `<GraphQL Remove=…>` entries intended to exclude `GetAvailablePorts`, `GetShips`, and a nonexistent `GetCruiseDetail` query — but the paths reference `src/GraphQL\*.graphql` while the actual files live under `src/GraphQL\Queries\*.graphql`, so the excludes don't match anything and are ineffective. All 4 files under `GraphQL/Queries/` (`GetAvailablePorts`, `GetFeaturedCruises`, `GetShips`, `SearchCruises`) currently generate client code. Fix the `Remove` paths if the intent to limit codegen still holds.
- **Mapping**: Use manual mapping methods in `MapperConfiguration.cs` to convert StrawberryShake generated types to local DTOs. Never map manually in page code-behind.
- **Validation**: FluentValidation validators are defined as nested `AbstractValidator<T>` classes inside the page's `.razor.cs` file (not separate files). They expose a `ValidateValue` func for MudBlazor's `For` / `Validation` binding pattern.
- **State**: `BookingStateService` is `Scoped` (per SignalR circuit) and in-memory only. It holds the multi-step booking wizard state (cruise → cabins → extras → payment). Call `Clear()` after booking completes.
- **Token handling**: `AuthHeaderHandler` (DelegatingHandler registered on Refit) reads the access token from the cookie auth ticket, refreshes it against Keycloak when within 1 minute of expiry, updates `AuthTokenStateService` (in-memory cache to avoid repeated cookie reads on the same Blazor circuit), and attaches it as `Bearer` on every Refit request.
- **Auth endpoints**: `/login` challenges OIDC; `/login-callback` upserts the user in the backend then redirects; `/logout` signs out both cookie and OIDC schemes. `Routes.razor` redirects unauthenticated users to `/login` via `RedirectToLogin`.
- **OIDC client secret**: `Program.cs` reads it from config (`Keycloak:ClientSecret`, `cruises-ui` client) and throws if missing. Placeholder value lives in `appsettings.json` — use `dotnet user-secrets` for real deployments.
- **AI chat**: `Chat` page calls `IChatApi.StreamChat` (Refit, `Integrations/IChatApi.cs`) directly — bypasses `IBookingApiService`/`ApiResult`, since it streams Server-Sent Events (`SseParser`) instead of returning a single response. Each circuit gets a fresh `Guid` session id on `OnInitializedAsync`. Assistant replies render as Markdown via Markdig (`RenderMarkdown`).
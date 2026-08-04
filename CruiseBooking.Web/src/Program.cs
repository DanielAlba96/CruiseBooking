using CruiseBooking.Components;
using CruiseBooking.Endpoints;
using CruiseBooking.Integrations;
using CruiseBooking.Services;
using CruiseBooking.Services.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using Refit;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddScoped<BookingStateService>();
builder.Services.AddScoped<AuthTokenStateService>();

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

var dataApiUrl = builder.Configuration["DataApi:Url"]
    ?? throw new InvalidOperationException("Missing configuration: DataApi:Url");
var bookingApiUrl = builder.Configuration["BookingApi:Url"]
    ?? throw new InvalidOperationException("Missing configuration: BookingApi:Url");

builder.Services.AddCruisesClient()
     .ConfigureHttpClient(client => client.BaseAddress = new Uri(dataApiUrl));

builder.Services.AddRefitClient<IBookingApi>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(bookingApiUrl);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped<IBookingApiService, BookingApiService>();

builder.Services.AddRefitClient<IChatApi>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(bookingApiUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.ExpectContinue = false;
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = builder.Configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Missing configuration: Keycloak:Authority");
        options.ClientId = builder.Configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Missing configuration: Keycloak:ClientId");
        options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"]
            ?? throw new InvalidOperationException("Missing configuration: Keycloak:ClientSecret");
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.SignedOutRedirectUri = "/";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.Scope.Add("address");
        options.Scope.Add("phone");
        options.Scope.Add("offline_access");

        if (builder.Environment.IsDevelopment())
            options.RequireHttpsMetadata = false;

        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "preferred_username");
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => options.LogoutPath = "/");

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthenticationEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();

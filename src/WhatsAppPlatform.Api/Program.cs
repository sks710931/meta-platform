using WhatsAppPlatform.Infrastructure;
using WhatsAppPlatform.Infrastructure.Meta;
using WhatsAppPlatform.Api.Identity;
using WhatsAppPlatform.Infrastructure.Identity;
using WhatsAppPlatform.Api.WhatsAppAccounts;
using WhatsAppPlatform.Application.WhatsAppAccounts.StartOnboardingSession;
using WhatsAppPlatform.Application.WhatsAppAccounts.GetOnboardingSession;
using WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;
using WhatsAppPlatform.Application.WhatsAppAccounts.ListWhatsAppAccounts;
using WhatsAppPlatform.Application.WhatsAppAccounts.GetWhatsAppAccount;
using WhatsAppPlatform.Api.Organizations;
using WhatsAppPlatform.Application.Organizations.CreateOrganization;
using WhatsAppPlatform.Application.Organizations.ListOrganizations;
using WhatsAppPlatform.Application.Organizations.GetOrganizationDetails;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Platform")
    ?? throw new InvalidOperationException("Configure ConnectionStrings__Platform before starting the API.");

builder.Services.AddInfrastructure(connectionString);
builder.AddPlatformAuthentication();
builder.Services.AddMetaEmbeddedSignup(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CreateOrganizationHandler>();
builder.Services.AddScoped<ListOrganizationsHandler>();
builder.Services.AddScoped<GetOrganizationDetailsHandler>();

builder.Services.AddSingleton(new OnboardingSessionSettings(TimeSpan.FromMinutes(
    builder.Configuration.GetValue<int>("WhatsAppOnboarding:SessionLifetimeMinutes", 15))));
builder.Services.AddScoped<StartOnboardingSessionHandler>();
builder.Services.AddScoped<GetOnboardingSessionHandler>();
builder.Services.AddScoped<RegisterOnboardingResultHandler>();
builder.Services.AddScoped<ListWhatsAppAccountsHandler>();
builder.Services.AddScoped<GetWhatsAppAccountHandler>();

var app = builder.Build();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseMiddleware<AntiforgeryMiddleware>();
await using (var scope = app.Services.CreateAsyncScope())
{
    var provisioner = scope.ServiceProvider.GetRequiredService<BootstrapIdentityProvisioner>();
    await provisioner.ProvisionAsync(app.Lifetime.ApplicationStopping);
    if (app.Environment.IsDevelopment()) await provisioner.ProvisionDevelopmentMembershipAsync(app.Lifetime.ApplicationStopping);
}
app.MapAuthentication();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
var organizations = app.MapGroup("/api/organizations").RequireAuthorization();
organizations.MapCreateOrganization();
organizations.MapListOrganizations();
organizations.MapGetOrganizationDetails();
app.MapStartOnboardingSession();
app.MapGetOnboardingSession();
app.MapDevelopmentOnlyCompletion();
app.MapMetaCompletion();
app.MapMetaSignupConfiguration();
app.MapListWhatsAppAccounts();
app.MapGetWhatsAppAccount();
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");
app.Run();

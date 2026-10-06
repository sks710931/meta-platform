using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using WhatsAppPlatform.Application.Identity;
using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Infrastructure.Identity;

namespace WhatsAppPlatform.Api.Identity;

internal static class IdentityRegistration
{
    public static void AddPlatformAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentityPersistence();
        var protection = builder.Services.AddDataProtection().SetApplicationName("WhatsAppPlatform");
        var keyDirectory = builder.Configuration["DataProtection:KeyDirectory"];
        if (!string.IsNullOrWhiteSpace(keyDirectory)) protection.PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
        else if (!builder.Environment.IsDevelopment())
            throw new InvalidOperationException("Configure DataProtection__KeyDirectory for a protected persistent key ring outside Development.");
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
        builder.Services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();
        builder.Services.AddScoped<CookieIdentitySession>();
        builder.Services.AddSingleton<UnknownUserPasswordCheck>();
        builder.Services.AddScoped<OrganizationAccessFilter>();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
        }).AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            options.Cookie.Name = builder.Environment.IsDevelopment() ? "platform-auth" : "__Host-platform-auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = false;
            options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
            options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
            options.Events.OnValidatePrincipal = async context =>
            {
                if (!context.Properties.Items.TryGetValue("platform-session-deadline", out var deadlineValue) ||
                    !DateTimeOffset.TryParse(deadlineValue, out var deadline) ||
                    deadline <= context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow())
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    return;
                }
                await SecurityStampValidator.ValidatePrincipalAsync(context);
                context.Properties.ExpiresUtc = deadline;
            };
        }).AddCookie(IdentityConstants.ExternalScheme, options => ConfigureFrameworkCookie(options, builder.Environment, "external"))
          .AddCookie(IdentityConstants.TwoFactorUserIdScheme, options => ConfigureFrameworkCookie(options, builder.Environment, "two-factor"));
        builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(5));
        builder.Services.AddAuthorization(options => options.AddPolicy("PlatformAdmin", policy =>
            policy.RequireAuthenticatedUser().RequireRole("PlatformAdmin")));
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = builder.Environment.IsDevelopment() ? "platform-csrf" : "__Host-platform-csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        });
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
        });
        builder.Services.AddScoped<BootstrapIdentityProvisioner>();
    }
    // SignInManager's security-stamp rejection signs out these framework schemes too.
    // No external provider or MFA login flow is registered; these cookies are never issued.
    private static void ConfigureFrameworkCookie(CookieAuthenticationOptions options, IWebHostEnvironment environment, string purpose)
    {
        options.Cookie.Name = environment.IsDevelopment() ? $"platform-{purpose}" : $"__Host-platform-{purpose}";
        options.Cookie.Path = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
    }

}

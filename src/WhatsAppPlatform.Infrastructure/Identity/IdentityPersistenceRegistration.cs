using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Identity;

public static class IdentityPersistenceRegistration
{
    public static void AddIdentityPersistence(this IServiceCollection services)
    {
        services.AddIdentityCore<PlatformUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
        }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<PlatformDbContext>().AddSignInManager();
    }
}

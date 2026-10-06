using Microsoft.EntityFrameworkCore;
using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Infrastructure.Identity;
using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.WhatsAppAccounts;
using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Infrastructure.Organizations;
using Microsoft.Extensions.DependencyInjection;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContext<PlatformDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", DatabaseSchemas.Platform)));
        services.AddScoped<IOrganizationStore, EfOrganizationStore>();
        services.AddScoped<IWhatsAppAccountStore, EfWhatsAppAccountStore>();
        services.AddScoped<IMembershipStore, EfMembershipStore>();
        services.AddScoped<ITenantResourceOwnership, EfTenantResourceOwnership>();
        return services;
    }
}

using Microsoft.EntityFrameworkCore;
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
        return services;
    }
}

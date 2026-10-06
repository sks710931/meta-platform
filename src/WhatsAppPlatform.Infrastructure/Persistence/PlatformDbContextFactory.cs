using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WhatsAppPlatform.Infrastructure.Persistence;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Platform")
            ?? throw new InvalidOperationException("Configure ConnectionStrings__Platform for EF migration commands.");
        var options = new DbContextOptionsBuilder<PlatformDbContext>();
        options.UseNpgsql(connectionString,
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", DatabaseSchemas.Platform));
        return new PlatformDbContext(options.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.WhatsAppAccounts;

namespace WhatsAppPlatform.Infrastructure.Persistence;

public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DatabaseSchemas.Platform);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
        // Cross-cutting relational integrity bridge; no Organization domain behavior or navigation graph.
        modelBuilder.Entity<EmbeddedSignupSession>().HasOne<Organization>().WithMany()
            .HasForeignKey(session => session.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<WhatsAppAccount>().HasOne<Organization>().WithMany()
            .HasForeignKey(account => account.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WhatsAppPlatform.Infrastructure.Identity;

internal static class IdentityModelConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<PlatformUser>().ToTable("users", "identity");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", "identity");
        builder.Entity<PlatformUser>().HasIndex(user => user.NormalizedEmail).IsUnique();
        foreach (var entity in builder.Model.GetEntityTypes().Where(entity => entity.GetSchema() == "identity"))
            foreach (var key in entity.GetForeignKeys()) key.DeleteBehavior = DeleteBehavior.Restrict;
    }
}

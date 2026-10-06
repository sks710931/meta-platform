using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts;

internal sealed class EmbeddedSignupSessionConfiguration : IEntityTypeConfiguration<EmbeddedSignupSession>
{
    public void Configure(EntityTypeBuilder<EmbeddedSignupSession> builder)
    {
        builder.ToTable("embedded_signup_sessions", DatabaseSchemas.WhatsApp, table =>
        {
            table.HasCheckConstraint("ck_signup_status", "status IN ('Pending', 'Completed', 'Failed', 'Expired')");
            table.HasCheckConstraint("ck_signup_expiry", "expires_at > started_at");
            table.HasCheckConstraint("ck_signup_completion", "(status = 'Completed' AND completed_at IS NOT NULL AND completed_at >= started_at AND completed_at < expires_at) OR (status <> 'Completed' AND completed_at IS NULL)");
        });
        builder.HasKey(session => session.Id);
        builder.HasAlternateKey(session => new { session.Id, session.OrganizationId });
        builder.Property(session => session.Id).HasColumnName("id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new EmbeddedSignupSessionId(value)).ValueGeneratedNever();
        builder.Property(session => session.OrganizationId).HasColumnName("organization_id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(session => session.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();
        builder.Property(session => session.StartedAt).HasColumnName("started_at").HasColumnType("timestamp with time zone");
        builder.Property(session => session.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp with time zone");
        builder.Property(session => session.CompletedAt).HasColumnName("completed_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(session => new { session.OrganizationId, session.StartedAt });
    }
}

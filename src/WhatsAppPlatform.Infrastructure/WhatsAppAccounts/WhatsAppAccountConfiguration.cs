using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts;

internal sealed class WhatsAppAccountConfiguration : IEntityTypeConfiguration<WhatsAppAccount>
{
    public void Configure(EntityTypeBuilder<WhatsAppAccount> builder)
    {
        builder.ToTable("accounts", DatabaseSchemas.WhatsApp, table =>
        {
            table.HasCheckConstraint("ck_account_status", "status IN ('Pending', 'Connected', 'Suspended', 'Disconnected')");
            table.HasCheckConstraint("ck_account_external_id", "external_whatsapp_account_id ~ '^[1-9][0-9]{0,99}$'");
            table.HasCheckConstraint("ck_account_name", "length(btrim(display_name)) > 0");
            table.HasCheckConstraint("ck_account_connected", "status <> 'Connected' OR (connected_at IS NOT NULL AND connected_at >= created_at)");
        });
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasColumnName("id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new WhatsAppAccountId(value)).ValueGeneratedNever();
        builder.Property(account => account.OrganizationId).HasColumnName("organization_id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(account => account.SignupSessionId).HasColumnName("signup_session_id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new EmbeddedSignupSessionId(value));
        builder.Property(account => account.ExternalWhatsAppAccountId).HasColumnName("external_whatsapp_account_id").HasMaxLength(100)
            .HasConversion(id => id.Value, value => RestoreExternalId(value));
        builder.Property(account => account.DisplayName).HasColumnName("display_name").HasMaxLength(200);
        builder.Property(account => account.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(account => account.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(account => account.ConnectedAt).HasColumnName("connected_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(account => account.ExternalWhatsAppAccountId).IsUnique();
        builder.HasIndex(account => account.SignupSessionId).IsUnique();
        builder.HasIndex(account => new { account.OrganizationId, account.CreatedAt, account.Id }).IsDescending(false, true, true);
        builder.HasOne<EmbeddedSignupSession>().WithMany()
            .HasForeignKey(account => new { account.SignupSessionId, account.OrganizationId })
            .HasPrincipalKey(session => new { session.Id, session.OrganizationId }).OnDelete(DeleteBehavior.Restrict);
    }

    private static ExternalWhatsAppAccountId RestoreExternalId(string value) =>
        ExternalWhatsAppAccountId.TryCreate(value, out var id) && id is not null ? id
            : throw new InvalidOperationException("Invalid stored external account identifier.");
}

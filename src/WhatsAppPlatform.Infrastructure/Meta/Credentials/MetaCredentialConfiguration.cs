using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Meta.Credentials;

internal sealed class MetaCredentialConfiguration : IEntityTypeConfiguration<MetaCredentialRecord>
{
    public void Configure(EntityTypeBuilder<MetaCredentialRecord> builder)
    {
        builder.ToTable("meta_credentials", DatabaseSchemas.WhatsApp, table =>
        {
            table.HasCheckConstraint("ck_meta_credential_time", "updated_at >= created_at");
            table.HasCheckConstraint("ck_meta_credential_protected", "protected_credential IS NULL OR length(protected_credential) > 0");
        });
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(record => record.SessionId).HasColumnName("signup_session_id")
            .HasConversion(id => id.Value, value => new EmbeddedSignupSessionId(value));
        builder.Property(record => record.ProtectedCredential).HasColumnName("protected_credential").HasColumnType("text");
        builder.Property(record => record.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(record => record.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(record => record.SessionId).IsUnique();
        builder.HasOne<EmbeddedSignupSession>().WithMany().HasForeignKey(record => record.SessionId).OnDelete(DeleteBehavior.Restrict);
    }
}

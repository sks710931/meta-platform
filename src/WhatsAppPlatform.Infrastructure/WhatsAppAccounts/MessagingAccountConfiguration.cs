using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.Messaging.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts;

internal sealed class MessagingAccountConfiguration : IEntityTypeConfiguration<MessagingAccount>
{
    public void Configure(EntityTypeBuilder<MessagingAccount> builder)
    {
        builder.ToTable("messaging_accounts", DatabaseSchemas.WhatsApp, table =>
            table.HasCheckConstraint("ck_messaging_external_id", "external_messaging_account_id ~ '^[1-9][0-9]{0,99}$'"));
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasColumnName("id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new MessagingAccountId(value)).ValueGeneratedNever();
        builder.Property(account => account.WhatsAppAccountId).HasColumnName("whatsapp_account_id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new WhatsAppAccountId(value));
        builder.Property(account => account.ExternalMessagingAccountId).HasColumnName("external_messaging_account_id").HasMaxLength(100)
            .HasConversion(id => id.Value, value => RestoreExternalId(value));
        builder.Property(account => account.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(account => account.ExternalMessagingAccountId).IsUnique();
        builder.HasIndex(account => account.WhatsAppAccountId).IsUnique();
        builder.HasOne<WhatsAppAccount>().WithMany().HasForeignKey(account => account.WhatsAppAccountId).OnDelete(DeleteBehavior.Restrict);
    }

    private static ExternalMessagingAccountId RestoreExternalId(string value) =>
        ExternalMessagingAccountId.TryCreate(value, out var id) && id is not null ? id
            : throw new InvalidOperationException("Invalid stored external messaging identifier.");
}

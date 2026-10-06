using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts;

internal sealed class MessagingAccountConfiguration : IEntityTypeConfiguration<MessagingAccount>
{
    public void Configure(EntityTypeBuilder<MessagingAccount> builder)
    {
        builder.ToTable("messaging_accounts", DatabaseSchemas.WhatsApp, table =>
            table.HasCheckConstraint("ck_messaging_external_id", """char_length(external_messaging_account_id) BETWEEN 1 AND 100 AND btrim(external_messaging_account_id, U&'\0020\00A0\1680\2000\2001\2002\2003\2004\2005\2006\2007\2008\2009\200A\2028\2029\202F\205F\3000') <> '' AND external_messaging_account_id !~ U&'[\0001-\001F\007F-\009F]'"""));
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

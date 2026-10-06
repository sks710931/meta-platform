using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts;

internal sealed class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder.ToTable("phone_numbers", DatabaseSchemas.WhatsApp, table =>
        {
            table.HasCheckConstraint("ck_phone_external_id", """char_length(external_phone_number_id) BETWEEN 1 AND 100 AND btrim(external_phone_number_id, U&'\0020\00A0\1680\2000\2001\2002\2003\2004\2005\2006\2007\2008\2009\200A\2028\2029\202F\205F\3000') <> '' AND external_phone_number_id !~ U&'[\0001-\001F\007F-\009F]'""");
            table.HasCheckConstraint("ck_phone_display", "length(btrim(display_phone_number)) > 0");
            table.HasCheckConstraint("ck_phone_status", "status = 'Registered'");
        });
        builder.HasKey(phone => phone.Id);
        builder.Property(phone => phone.Id).HasColumnName("id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new PhoneNumberId(value)).ValueGeneratedNever();
        builder.Property(phone => phone.WhatsAppAccountId).HasColumnName("whatsapp_account_id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new WhatsAppAccountId(value));
        builder.Property(phone => phone.ExternalPhoneNumberId).HasColumnName("external_phone_number_id").HasMaxLength(100)
            .HasConversion(id => id.Value, value => RestoreExternalId(value));
        builder.Property(phone => phone.DisplayPhoneNumber).HasColumnName("display_phone_number").HasMaxLength(50);
        builder.Property(phone => phone.VerifiedName).HasColumnName("verified_name").HasMaxLength(200);
        builder.Property(phone => phone.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(phone => phone.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(phone => phone.ExternalPhoneNumberId).IsUnique();
        builder.HasOne<WhatsAppAccount>().WithMany().HasForeignKey(phone => phone.WhatsAppAccountId).OnDelete(DeleteBehavior.Restrict);
    }

    private static ExternalPhoneNumberId RestoreExternalId(string value) =>
        ExternalPhoneNumberId.TryCreate(value, out var id) && id is not null ? id
            : throw new InvalidOperationException("Invalid stored external phone identifier.");
}

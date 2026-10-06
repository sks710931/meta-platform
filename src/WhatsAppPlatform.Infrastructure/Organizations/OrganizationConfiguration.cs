using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Organizations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations", DatabaseSchemas.Organizations, table =>
        {
            table.HasCheckConstraint("ck_organizations_name", "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_organizations_status", "status IN ('Active', 'Suspended')");
        });
        builder.HasKey(organization => organization.Id);
        builder.Property(organization => organization.Id)
            .HasColumnName("id").HasColumnType("uuid")
            .HasConversion(id => id.Value, value => new OrganizationId(value)).ValueGeneratedNever();
        builder.Property(organization => organization.Name)
            .HasColumnName("name").HasMaxLength(Organization.MaximumNameLength).IsRequired();
        builder.Property(organization => organization.Status)
            .HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(organization => organization.CreatedAt)
            .HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(organization => new { organization.CreatedAt, organization.Id }).IsDescending(true, true);
    }
}

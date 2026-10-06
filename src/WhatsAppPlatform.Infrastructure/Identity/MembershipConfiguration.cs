using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Infrastructure.Identity;

internal sealed class MembershipConfiguration : IEntityTypeConfiguration<MembershipRecord>
{
    public void Configure(EntityTypeBuilder<MembershipRecord> builder)
    {
        builder.ToTable("organization_memberships", "identity", table =>
            table.HasCheckConstraint("ck_membership_role", "role IN ('OrganizationAdmin', 'Member')"));
        builder.HasKey(membership => membership.Id);
        builder.Property(membership => membership.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(membership => membership.OrganizationId).HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(membership => membership.UserId).HasColumnName("user_id");
        builder.Property(membership => membership.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(32);
        builder.Property(membership => membership.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(membership => new { membership.OrganizationId, membership.UserId }).IsUnique();
        builder.HasIndex(membership => membership.UserId);
        builder.HasOne<Organization>().WithMany().HasForeignKey(membership => membership.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlatformUser>().WithMany().HasForeignKey(membership => membership.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

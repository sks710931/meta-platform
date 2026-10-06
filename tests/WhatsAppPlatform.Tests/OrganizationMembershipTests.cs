using WhatsAppPlatform.Domain.Identity;
using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class OrganizationMembershipTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly OrganizationMembershipId Id = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    private static readonly OrganizationId Organization = new(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    private static readonly UserId User = new(Guid.Parse("33333333-3333-3333-3333-333333333333"));

    [Fact]
    public void Membership_binds_a_valid_role_to_one_user_and_organization_at_utc_time()
    {
        Assert.True(OrganizationMembership.TryCreate(Id, Organization, User, OrganizationRole.OrganizationAdmin, Now, out var membership));
        Assert.NotNull(membership);
        Assert.Equal(Organization, membership.OrganizationId);
        Assert.Equal(User, membership.UserId);
        Assert.Equal(OrganizationRole.OrganizationAdmin, membership.Role);
        Assert.Equal(TimeSpan.Zero, membership.CreatedAt.Offset);
    }

    [Fact]
    public void Invalid_membership_role_or_non_utc_timestamp_is_rejected()
    {
        Assert.False(OrganizationMembership.TryCreate(Id, Organization, User, (OrganizationRole)99, Now, out _));
        Assert.False(OrganizationMembership.TryCreate(Id, Organization, User, OrganizationRole.Member, Now.ToOffset(TimeSpan.FromHours(1)), out _));
    }
}

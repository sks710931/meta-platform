using WhatsAppPlatform.Application.Identity;
using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class OrganizationAccessServiceTests
{
    private static readonly UserId Alice = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    private static readonly UserId Bob = new(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    private static readonly OrganizationId First = new(Guid.Parse("33333333-3333-3333-3333-333333333333"));
    private static readonly OrganizationId Second = new(Guid.Parse("44444444-4444-4444-4444-444444444444"));
    private static OrganizationAccessService Access(UserId? user, bool admin = false) =>
        new(new TestUser(user, admin), new MembershipFixture());

    [Fact]
    public async Task Member_reads_own_organization_but_cannot_administer_or_access_another_tenant()
    {
        var access = Access(Alice);
        Assert.True(await access.CanAccessAsync(First, CancellationToken.None));
        Assert.False(await access.CanAdministerAsync(First, CancellationToken.None));
        Assert.False(await access.CanAccessAsync(Second, CancellationToken.None));
        Assert.False(await access.CanAdministerAsync(Second, CancellationToken.None));
    }

    [Fact]
    public async Task Organization_admin_has_no_platform_wide_authority()
    {
        var access = Access(Bob);
        Assert.True(await access.CanAdministerAsync(Second, CancellationToken.None));
        Assert.False(await access.CanAccessAsync(First, CancellationToken.None));
    }

    [Fact]
    public async Task Platform_admin_bypasses_membership_and_gets_unrestricted_catalog_scope()
    {
        var access = Access(Alice, true);
        Assert.True(await access.CanAccessAsync(Second, CancellationToken.None));
        Assert.True(await access.CanAdministerAsync(Second, CancellationToken.None));
        Assert.Null(await access.AccessibleOrganizationIdsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Anonymous_user_is_denied_even_if_an_admin_flag_is_supplied()
    {
        var access = Access(null, true);
        Assert.False(await access.CanAccessAsync(First, CancellationToken.None));
        Assert.False(await access.CanAdministerAsync(First, CancellationToken.None));
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<OrganizationId>>(
            await access.AccessibleOrganizationIdsAsync(CancellationToken.None)));
    }

    [Fact]
    public async Task Catalog_scope_contains_only_the_current_users_memberships()
    {
        Assert.Equal(new[] { First }, await Access(Alice).AccessibleOrganizationIdsAsync(CancellationToken.None));
        Assert.Equal(new[] { Second }, await Access(Bob).AccessibleOrganizationIdsAsync(CancellationToken.None));
    }

    private sealed record TestUser(UserId? UserId, bool IsPlatformAdmin) : ICurrentUser
    {
        public string? Email => null;
    }

    // Small application-port fixture, not a simulation or test of EF/database persistence.
    private sealed class MembershipFixture : IMembershipStore
    {
        private static OrganizationRole? Role(UserId user, OrganizationId organization) =>
            user == Alice && organization == First ? OrganizationRole.Member :
            user == Bob && organization == Second ? OrganizationRole.OrganizationAdmin : null;

        public Task<OrganizationRole?> FindRoleAsync(UserId userId, OrganizationId organizationId, CancellationToken cancellationToken) =>
            Task.FromResult(Role(userId, organizationId));
        public Task<IReadOnlyList<OrganizationId>> ListOrganizationIdsAsync(UserId userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<OrganizationId>>(new[] { First, Second }.Where(org => Role(userId, org) is not null).ToArray());
        public Task<IReadOnlyList<UserOrganization>> ListOrganizationsAsync(UserId userId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("This access-decision fixture does not supply UI profiles.");
    }
}

using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Identity;

public sealed class OrganizationAccessService(ICurrentUser currentUser, IMembershipStore memberships) : IOrganizationAccessService
{
    private Task<OrganizationRole?> RoleAsync(OrganizationId organizationId, CancellationToken cancellationToken) =>
        currentUser.UserId is { } userId && !currentUser.IsPlatformAdmin
            ? memberships.FindRoleAsync(userId, organizationId, cancellationToken)
            : Task.FromResult<OrganizationRole?>(null);

    public async Task<bool> CanAccessAsync(OrganizationId organizationId, CancellationToken cancellationToken) =>
        OrganizationAccessDecision.CanAccess(currentUser.UserId, currentUser.IsPlatformAdmin,
            await RoleAsync(organizationId, cancellationToken));

    public async Task<bool> CanAdministerAsync(OrganizationId organizationId, CancellationToken cancellationToken) =>
        OrganizationAccessDecision.CanAdminister(currentUser.UserId, currentUser.IsPlatformAdmin,
            await RoleAsync(organizationId, cancellationToken));

    public async Task<IReadOnlyList<OrganizationId>?> AccessibleOrganizationIdsAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId) return [];
        return currentUser.IsPlatformAdmin ? null : await memberships.ListOrganizationIdsAsync(userId, cancellationToken);
    }
}

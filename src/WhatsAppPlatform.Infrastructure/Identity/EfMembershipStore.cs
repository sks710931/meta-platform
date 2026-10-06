using Microsoft.EntityFrameworkCore;
using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Identity;

internal sealed class EfMembershipStore(PlatformDbContext context) : IMembershipStore
{
    public Task<OrganizationRole?> FindRoleAsync(UserId userId, OrganizationId organizationId, CancellationToken cancellationToken) =>
        context.Set<MembershipRecord>().AsNoTracking().Where(row => row.UserId == userId.Value && row.OrganizationId == organizationId)
            .Select(row => (OrganizationRole?)row.Role).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<OrganizationId>> ListOrganizationIdsAsync(UserId userId, CancellationToken cancellationToken) =>
        await context.Set<MembershipRecord>().AsNoTracking().Where(row => row.UserId == userId.Value)
            .Select(row => row.OrganizationId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserOrganization>> ListOrganizationsAsync(UserId userId, CancellationToken cancellationToken)
    {
        var rows = await (from membership in context.Set<MembershipRecord>().AsNoTracking()
                          join organization in context.Set<Organization>().AsNoTracking() on membership.OrganizationId equals organization.Id
                          where membership.UserId == userId.Value
                          orderby organization.Name, organization.Id
                          select new { organization.Id, organization.Name, membership.Role }).ToListAsync(cancellationToken);
        return rows.Select(row => new UserOrganization(row.Id.Value, row.Name, row.Role.ToString())).ToArray();
    }
}

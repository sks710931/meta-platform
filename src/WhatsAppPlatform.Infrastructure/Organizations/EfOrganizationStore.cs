using Microsoft.EntityFrameworkCore;
using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Organizations;

internal sealed class EfOrganizationStore(PlatformDbContext context) : IOrganizationStore
{
    public async Task AddAsync(Organization organization, CancellationToken cancellationToken)
    {
        context.Set<Organization>().Add(organization);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Organization>> ListNewestFirstAsync(IReadOnlyList<OrganizationId>? accessibleIds, CancellationToken cancellationToken) =>
        await context.Set<Organization>().AsNoTracking()
            .Where(organization => accessibleIds == null || accessibleIds.Contains(organization.Id))
            .OrderByDescending(organization => organization.CreatedAt)
            .ThenByDescending(organization => organization.Id)
            .ToListAsync(cancellationToken);

    public Task<Organization?> FindAsync(OrganizationId organizationId, CancellationToken cancellationToken) =>
        context.Set<Organization>().AsNoTracking()
            .SingleOrDefaultAsync(organization => organization.Id == organizationId, cancellationToken);
}

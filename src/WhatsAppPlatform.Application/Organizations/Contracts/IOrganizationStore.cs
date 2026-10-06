using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Organizations.Contracts;

public interface IOrganizationStore
{
    Task AddAsync(Organization organization, CancellationToken cancellationToken);
    Task<IReadOnlyList<Organization>> ListNewestFirstAsync(CancellationToken cancellationToken);
    Task<Organization?> FindAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}

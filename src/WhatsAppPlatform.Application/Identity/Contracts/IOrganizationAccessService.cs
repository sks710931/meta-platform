using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Identity.Contracts;

public interface IOrganizationAccessService
{
    Task<bool> CanAccessAsync(OrganizationId organizationId, CancellationToken cancellationToken);
    Task<bool> CanAdministerAsync(OrganizationId organizationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrganizationId>?> AccessibleOrganizationIdsAsync(CancellationToken cancellationToken);
}

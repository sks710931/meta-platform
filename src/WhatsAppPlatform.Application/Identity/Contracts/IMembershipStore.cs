using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Identity.Contracts;

public sealed record UserOrganization(Guid OrganizationId, string OrganizationName, string Role);

public interface IMembershipStore
{
    Task<OrganizationRole?> FindRoleAsync(UserId userId, OrganizationId organizationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrganizationId>> ListOrganizationIdsAsync(UserId userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserOrganization>> ListOrganizationsAsync(UserId userId, CancellationToken cancellationToken);
}

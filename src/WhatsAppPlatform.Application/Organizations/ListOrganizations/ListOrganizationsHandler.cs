using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Application.Identity.Contracts;

namespace WhatsAppPlatform.Application.Organizations.ListOrganizations;

public sealed class ListOrganizationsHandler(IOrganizationStore store, IOrganizationAccessService access)
{
    public async Task<IReadOnlyList<OrganizationResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var organizations = await store.ListNewestFirstAsync(await access.AccessibleOrganizationIdsAsync(cancellationToken), cancellationToken);
        return organizations.Select(OrganizationResponse.FromOrganization).ToArray();
    }
}

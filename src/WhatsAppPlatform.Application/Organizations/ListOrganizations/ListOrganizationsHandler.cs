using WhatsAppPlatform.Application.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Organizations.ListOrganizations;

public sealed class ListOrganizationsHandler(IOrganizationStore store)
{
    public async Task<IReadOnlyList<OrganizationResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var organizations = await store.ListNewestFirstAsync(cancellationToken);
        return organizations.Select(OrganizationResponse.FromOrganization).ToArray();
    }
}

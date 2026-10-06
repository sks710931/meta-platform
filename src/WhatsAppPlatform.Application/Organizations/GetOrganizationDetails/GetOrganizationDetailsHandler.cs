using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Organizations.GetOrganizationDetails;

public sealed class GetOrganizationDetailsHandler(IOrganizationStore store)
{
    public async Task<GetOrganizationDetailsResult> HandleAsync(
        OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var organization = await store.FindAsync(organizationId, cancellationToken);
        return organization is null
            ? new GetOrganizationDetailsResult.NotFound()
            : new GetOrganizationDetailsResult.Found(OrganizationResponse.FromOrganization(organization));
    }
}

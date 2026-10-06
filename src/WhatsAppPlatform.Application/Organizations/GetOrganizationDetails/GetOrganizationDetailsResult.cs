using WhatsAppPlatform.Application.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Organizations.GetOrganizationDetails;

public abstract record GetOrganizationDetailsResult
{
    private GetOrganizationDetailsResult() { }

    public sealed record Found(OrganizationResponse Organization) : GetOrganizationDetailsResult;
    public sealed record NotFound : GetOrganizationDetailsResult;
}

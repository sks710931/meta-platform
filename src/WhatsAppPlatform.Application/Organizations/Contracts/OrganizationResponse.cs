using WhatsAppPlatform.Domain.Organizations;

namespace WhatsAppPlatform.Application.Organizations.Contracts;

public sealed record OrganizationResponse(Guid OrganizationId, string Name, string Status, DateTimeOffset CreatedAt)
{
    public static OrganizationResponse FromOrganization(Organization organization) =>
        new(organization.Id.Value, organization.Name, organization.Status.ToString(), organization.CreatedAt);
}

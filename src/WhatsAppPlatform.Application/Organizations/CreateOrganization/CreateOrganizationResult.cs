using WhatsAppPlatform.Application.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Organizations.CreateOrganization;

public abstract record CreateOrganizationResult
{
    private CreateOrganizationResult() { }

    public sealed record Created(OrganizationResponse Organization) : CreateOrganizationResult;
    public sealed record Invalid(string Message) : CreateOrganizationResult;
}

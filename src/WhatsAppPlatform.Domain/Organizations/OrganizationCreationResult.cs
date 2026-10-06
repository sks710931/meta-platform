namespace WhatsAppPlatform.Domain.Organizations;

public abstract record OrganizationCreationResult
{
    private OrganizationCreationResult() { }

    public sealed record Created(Organization Organization) : OrganizationCreationResult;
    public sealed record Invalid(string Message) : OrganizationCreationResult;
}

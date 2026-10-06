using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.Organizations.CreateOrganization;

public sealed class CreateOrganizationHandler(IOrganizationStore store, TimeProvider clock)
{
    public async Task<CreateOrganizationResult> HandleAsync(string? name, CancellationToken cancellationToken)
    {
        var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(clock.GetUtcNow().ToUnixTimeMilliseconds());
        var result = Organization.Create(new OrganizationId(Guid.NewGuid()), name, createdAt);
        switch (result)
        {
            case OrganizationCreationResult.Invalid invalid:
                return new CreateOrganizationResult.Invalid(invalid.Message);
            case OrganizationCreationResult.Created created:
                await store.AddAsync(created.Organization, cancellationToken);
                return new CreateOrganizationResult.Created(OrganizationResponse.FromOrganization(created.Organization));
            default:
                throw new InvalidOperationException("Unknown organization creation result.");
        }
    }
}

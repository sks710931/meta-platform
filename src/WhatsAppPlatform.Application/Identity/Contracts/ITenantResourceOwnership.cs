using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.Identity.Contracts;

public interface ITenantResourceOwnership
{
    Task<OrganizationId?> AccountOrganizationAsync(WhatsAppAccountId accountId, CancellationToken cancellationToken);
    Task<OrganizationId?> SessionOrganizationAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken);
}

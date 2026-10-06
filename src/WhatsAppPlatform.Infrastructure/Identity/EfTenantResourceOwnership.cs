using Microsoft.EntityFrameworkCore;
using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Identity;

internal sealed class EfTenantResourceOwnership(PlatformDbContext context) : ITenantResourceOwnership
{
    public Task<OrganizationId?> AccountOrganizationAsync(WhatsAppAccountId accountId, CancellationToken cancellationToken) =>
        context.Set<WhatsAppAccount>().AsNoTracking().Where(account => account.Id == accountId)
            .Select(account => (OrganizationId?)account.OrganizationId).SingleOrDefaultAsync(cancellationToken);

    public Task<OrganizationId?> SessionOrganizationAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken) =>
        context.Set<EmbeddedSignupSession>().AsNoTracking().Where(session => session.Id == sessionId)
            .Select(session => (OrganizationId?)session.OrganizationId).SingleOrDefaultAsync(cancellationToken);
}

using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

public enum SaveOutcome { Saved, Conflict }

public interface IWhatsAppAccountStore
{
    Task AddSessionAsync(EmbeddedSignupSession session, CancellationToken cancellationToken);
    Task<EmbeddedSignupSession?> FindSessionAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken);
    Task<SaveOutcome> SaveSessionAsync(CancellationToken cancellationToken);
    Task<SaveOutcome> SaveRegistrationAsync(RegisteredAccount account, CancellationToken cancellationToken);
    Task<RegisteredAccount?> FindRegistrationAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken);
    Task<RegisteredAccount?> FindAccountAsync(WhatsAppAccountId accountId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RegisteredAccount>> ListAccountsAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}

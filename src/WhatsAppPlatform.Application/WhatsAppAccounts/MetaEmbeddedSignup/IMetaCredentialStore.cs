using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

public sealed record StoredMetaCredential(MetaCredential? Credential);
public interface IMetaCredentialStore
{
    // A reservation without ciphertext means exchange may have consumed its one-time code.
    Task<StoredMetaCredential?> FindAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken);
    Task<bool> ReserveExchangeAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken);
    Task<bool> ProtectAndPersistAsync(EmbeddedSignupSessionId sessionId, MetaCredential credential, CancellationToken cancellationToken);
}

public interface IOnboardingAttemptCoordinator
{
    Task<IAsyncDisposable?> TryAcquireAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken);
}

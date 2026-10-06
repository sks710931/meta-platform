using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

public sealed class CompleteMetaOnboardingHandler(IWhatsAppAccountStore accounts,
    IMetaEmbeddedSignupGateway gateway, IMetaCredentialStore credentials,
    IOnboardingAttemptCoordinator coordinator, RegisterOnboardingResultHandler register, TimeProvider clock)
{
    public async Task<OnboardingResult<RegistrationResponse>> HandleAsync(EmbeddedSignupSessionId sessionId,
        string? authorizationCode, CancellationToken cancellationToken)
    {
        if (!gateway.IsEnabled) return Failure(OnboardingError.Disabled, "Meta onboarding is not configured.");
        // Across replicas, serialize the entire exchange/checkpoint/discovery/registration attempt.
        await using var lease = await coordinator.TryAcquireAsync(sessionId, cancellationToken);
        if (lease is null) return Failure(OnboardingError.Conflict, "Onboarding is processing. Retry this session shortly.");
        var session = await accounts.FindSessionAsync(sessionId, cancellationToken);
        if (session is null) return Failure(OnboardingError.NotFound, "Session not found.");
        if (session.Status == EmbeddedSignupSessionStatus.Completed)
        {
            var existing = await accounts.FindRegistrationAsync(sessionId, cancellationToken);
            return existing is null ? Failure(OnboardingError.Conflict, "Session has no registered account.")
                : OnboardingResult<RegistrationResponse>.Success(new(OnboardingSessionResponse.FromSession(session),
                    WhatsAppAccountResponse.FromRegistration(existing), true));
        }
        if (session.ExpireIfDue(clock.GetUtcNow())) await accounts.SaveSessionAsync(cancellationToken);
        if (session.Status == EmbeddedSignupSessionStatus.Expired)
            return Failure(OnboardingError.Expired, "Session expired. Start a new session.");
        if (session.Status != EmbeddedSignupSessionStatus.Pending)
            return Failure(OnboardingError.Conflict, "Session cannot be completed.");
        var stored = await credentials.FindAsync(sessionId, cancellationToken);
        MetaCredential? credential = stored?.Credential;
        if (MetaCredentialReplay.Decide(stored) == MetaExchangeDecision.RestartRequired) return RestartRequired();
        if (MetaCredentialReplay.Decide(stored) == MetaExchangeDecision.ExchangeOnce)
        {
            if (string.IsNullOrWhiteSpace(authorizationCode) || authorizationCode.Length > 4096 || authorizationCode.Any(char.IsControl))
                return Failure(OnboardingError.Invalid, "A valid authorization result is required.");
            if (!await credentials.ReserveExchangeAsync(sessionId, cancellationToken)) return RestartRequired();
            var exchange = await gateway.ExchangeAsync(authorizationCode, cancellationToken);
            if (exchange.Value is null)
                return Failure(OnboardingError.RestartRequired, "Meta code exchange failed. Start a new signup; the code will not be retried.");
            credential = exchange.Value;
            if (!await credentials.ProtectAndPersistAsync(sessionId, credential, cancellationToken)) return RestartRequired();
        }
        var discovery = await gateway.DiscoverAsync(credential!, cancellationToken);
        if (discovery.Value is null)
            return Failure(discovery.Error == MetaGatewayError.Unavailable ? OnboardingError.ProviderUnavailable : OnboardingError.ProviderRejected,
                discovery.Error == MetaGatewayError.UnsupportedAssets
                    ? "Meta did not return one explicitly authorized Messaging Account with supported phone numbers. Start a new signup selecting one account."
                    : "Meta resource verification failed. Retry this session; the stored credential will be used without exchanging the code again.");
        var registration = await register.HandleAsync(sessionId, discovery.Value, cancellationToken);
        return registration.Error == OnboardingError.Invalid
            ? Failure(OnboardingError.ProviderRejected, "Discovered Meta resources cannot be represented safely. Start a new signup or contact an administrator.")
            : registration;
    }

    private static OnboardingResult<RegistrationResponse> RestartRequired() => Failure(OnboardingError.RestartRequired,
        "The authorization code may have been consumed but a usable credential was not saved. Start a new signup.");
    private static OnboardingResult<RegistrationResponse> Failure(OnboardingError error, string message) =>
        OnboardingResult<RegistrationResponse>.Failure(error, message);
}

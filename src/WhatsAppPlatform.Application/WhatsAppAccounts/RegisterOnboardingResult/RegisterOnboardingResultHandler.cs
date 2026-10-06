using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;

// Shared atomic account registration: inputs come from Development simulation or verified provider discovery.
public sealed class RegisterOnboardingResultHandler(IWhatsAppAccountStore store, TimeProvider clock)
{
    public async Task<OnboardingResult<RegistrationResponse>> HandleAsync(
        EmbeddedSignupSessionId sessionId, RegisterOnboardingInput input, CancellationToken cancellationToken)
    {
        var session = await store.FindSessionAsync(sessionId, cancellationToken);
        if (session is null) return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.NotFound, "Session not found.");
        var now = DateTimeOffset.FromUnixTimeMilliseconds(clock.GetUtcNow().ToUnixTimeMilliseconds());
        var draft = RegistrationDraft.Create(session, input, now);
        if (draft is null) return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.Invalid,
            "Provide nonempty external IDs up to 100 characters without control characters, a display name up to 200 characters, and 1–100 distinct phone numbers (display up to 50, verified name up to 200 characters).");
        if (session.Status == EmbeddedSignupSessionStatus.Completed)
            return await ReplayAsync(sessionId, draft, cancellationToken);
        var transition = session.Complete(now);
        if (transition == SignupTransition.Expired)
        {
            if (await store.SaveSessionAsync(cancellationToken) == SaveOutcome.Conflict)
                return await ReplayAsync(sessionId, draft, cancellationToken);
            return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.Expired, "Session expired. Start a new session.");
        }
        if (session.Status == EmbeddedSignupSessionStatus.Expired)
            return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.Expired, "Session expired. Start a new session.");
        if (transition != SignupTransition.Applied)
            return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.Conflict, "Session cannot be completed.");
        if (await store.SaveRegistrationAsync(draft, cancellationToken) == SaveOutcome.Conflict)
            return await ReplayAsync(sessionId, draft, cancellationToken);
        return OnboardingResult<RegistrationResponse>.Success(new(
            OnboardingSessionResponse.FromSession(session), WhatsAppAccountResponse.FromRegistration(draft), false));
    }

    private async Task<OnboardingResult<RegistrationResponse>> ReplayAsync(
        EmbeddedSignupSessionId sessionId, RegisteredAccount draft, CancellationToken cancellationToken)
    {
        var session = await store.FindSessionAsync(sessionId, cancellationToken);
        if (session?.Status == EmbeddedSignupSessionStatus.Expired)
            return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.Expired, "Session expired. Start a new session.");
        var existing = await store.FindRegistrationAsync(sessionId, cancellationToken);
        if (session?.Status == EmbeddedSignupSessionStatus.Completed && existing is not null && RegistrationDraft.Matches(existing, draft))
            return OnboardingResult<RegistrationResponse>.Success(new(
                OnboardingSessionResponse.FromSession(session), WhatsAppAccountResponse.FromRegistration(existing), true));
        return OnboardingResult<RegistrationResponse>.Failure(OnboardingError.Conflict,
            "Session result conflicts with an existing registration. No records were added.");
    }
}

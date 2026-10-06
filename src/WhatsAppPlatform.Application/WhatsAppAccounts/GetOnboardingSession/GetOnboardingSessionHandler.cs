using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.GetOnboardingSession;

public sealed class GetOnboardingSessionHandler(IWhatsAppAccountStore store, TimeProvider clock)
{
    public async Task<OnboardingResult<OnboardingSessionResponse>> HandleAsync(
        EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken)
    {
        var session = await store.FindSessionAsync(sessionId, cancellationToken);
        if (session is null) return OnboardingResult<OnboardingSessionResponse>.Failure(OnboardingError.NotFound, "Session not found.");
        if (session.ExpireIfDue(clock.GetUtcNow()) && await store.SaveSessionAsync(cancellationToken) == SaveOutcome.Conflict)
        {
            session = await store.FindSessionAsync(sessionId, cancellationToken)
                ?? throw new InvalidOperationException("Session disappeared during expiration.");
        }
        return OnboardingResult<OnboardingSessionResponse>.Success(OnboardingSessionResponse.FromSession(session));
    }
}

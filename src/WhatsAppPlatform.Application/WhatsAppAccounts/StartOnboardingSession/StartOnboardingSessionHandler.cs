using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.StartOnboardingSession;

public sealed class StartOnboardingSessionHandler(IOrganizationStore organizations, IWhatsAppAccountStore store,
    TimeProvider clock, OnboardingSessionSettings settings)
{
    public async Task<OnboardingResult<OnboardingSessionResponse>> HandleAsync(
        OrganizationId organizationId, CancellationToken cancellationToken)
    {
        if (await organizations.FindAsync(organizationId, cancellationToken) is null)
            return OnboardingResult<OnboardingSessionResponse>.Failure(OnboardingError.NotFound, "Organization not found.");
        var now = DateTimeOffset.FromUnixTimeMilliseconds(clock.GetUtcNow().ToUnixTimeMilliseconds());
        if (!EmbeddedSignupSession.TryStart(new EmbeddedSignupSessionId(Guid.NewGuid()), organizationId,
            now, now + settings.Lifetime, out var session) || session is null)
            throw new InvalidOperationException("Invalid onboarding session configuration.");
        await store.AddSessionAsync(session, cancellationToken);
        return OnboardingResult<OnboardingSessionResponse>.Success(OnboardingSessionResponse.FromSession(session));
    }
}

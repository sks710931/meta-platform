using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal sealed record OnboardingSessionView(Guid SessionId, Guid OrganizationId, string Status,
    DateTimeOffset StartedAt, DateTimeOffset ExpiresAt, DateTimeOffset? CompletedAt, bool ManualCompletionAvailable)
{
    public static OnboardingSessionView FromSession(OnboardingSessionResponse session, IHostEnvironment environment) =>
        new(session.SessionId, session.OrganizationId, session.Status, session.StartedAt, session.ExpiresAt,
            session.CompletedAt, environment.IsDevelopment());
}

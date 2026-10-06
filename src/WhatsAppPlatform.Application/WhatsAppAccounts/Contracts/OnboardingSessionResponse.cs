using WhatsAppPlatform.Domain.WhatsAppAccounts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

public sealed record OnboardingSessionResponse(Guid SessionId, Guid OrganizationId, string Status,
    DateTimeOffset StartedAt, DateTimeOffset ExpiresAt, DateTimeOffset? CompletedAt)
{
    public static OnboardingSessionResponse FromSession(EmbeddedSignupSession session) =>
        new(session.Id.Value, session.OrganizationId.Value, session.Status.ToString(), session.StartedAt, session.ExpiresAt, session.CompletedAt);
}

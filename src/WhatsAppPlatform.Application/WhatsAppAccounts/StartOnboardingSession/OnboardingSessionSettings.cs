namespace WhatsAppPlatform.Application.WhatsAppAccounts.StartOnboardingSession;

public sealed record OnboardingSessionSettings
{
    public TimeSpan Lifetime { get; }
    public OnboardingSessionSettings(TimeSpan lifetime)
    {
        if (lifetime < TimeSpan.FromMinutes(1) || lifetime > TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Session lifetime must be between 1 minute and 1 day.");
        Lifetime = lifetime;
    }
}

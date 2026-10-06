namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record EmbeddedSignupSessionId
{
    public Guid Value { get; }
    public EmbeddedSignupSessionId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Session ID must not be empty.", nameof(value));
        Value = value;
    }
}

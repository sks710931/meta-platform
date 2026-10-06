namespace WhatsAppPlatform.Domain.Identity.Contracts;

public sealed record UserId
{
    public Guid Value { get; }
    public UserId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("An internal identifier must not be empty.", nameof(value));
        Value = value;
    }
}

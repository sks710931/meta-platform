namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record WhatsAppAccountId
{
    public Guid Value { get; }

    public WhatsAppAccountId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("An internal identifier must not be empty.", nameof(value));
        }

        Value = value;
    }
}

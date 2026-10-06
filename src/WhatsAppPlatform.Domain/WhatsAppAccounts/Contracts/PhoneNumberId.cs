namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record PhoneNumberId
{
    public Guid Value { get; }

    public PhoneNumberId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("An internal identifier must not be empty.", nameof(value));
        }

        Value = value;
    }
}

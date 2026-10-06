namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record ExternalMessagingAccountId
{
    public string Value { get; }
    private ExternalMessagingAccountId(string value) => Value = value;

    public static bool TryCreate(string? value, out ExternalMessagingAccountId? identifier)
    {
        identifier = null;
        if (!ExternalIdentifierValidation.IsValid(value))
            return false;
        identifier = new ExternalMessagingAccountId(value);
        return true;
    }
}

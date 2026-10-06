namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record ExternalMessagingAccountId
{
    public string Value { get; }
    private ExternalMessagingAccountId(string value) => Value = value;

    public static bool TryCreate(string? value, out ExternalMessagingAccountId? identifier)
    {
        identifier = null;
        if (string.IsNullOrEmpty(value) || value.Length > 100 || value[0] == '0' || !value.All(char.IsAsciiDigit))
            return false;
        identifier = new ExternalMessagingAccountId(value);
        return true;
    }
}

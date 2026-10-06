namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record ExternalWhatsAppAccountId
{
    public string Value { get; }
    private ExternalWhatsAppAccountId(string value) => Value = value;

    public static bool TryCreate(string? value, out ExternalWhatsAppAccountId? identifier)
    {
        identifier = null;
        if (string.IsNullOrEmpty(value) || value.Length > 100 || value[0] == '0' || !value.All(char.IsAsciiDigit))
            return false;
        identifier = new ExternalWhatsAppAccountId(value);
        return true;
    }
}

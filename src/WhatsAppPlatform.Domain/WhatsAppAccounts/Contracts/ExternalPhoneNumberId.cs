namespace WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

public sealed record ExternalPhoneNumberId
{
    public string Value { get; }
    private ExternalPhoneNumberId(string value) => Value = value;

    public static bool TryCreate(string? value, out ExternalPhoneNumberId? identifier)
    {
        identifier = null;
        if (!ExternalIdentifierValidation.IsValid(value))
            return false;
        identifier = new ExternalPhoneNumberId(value);
        return true;
    }
}

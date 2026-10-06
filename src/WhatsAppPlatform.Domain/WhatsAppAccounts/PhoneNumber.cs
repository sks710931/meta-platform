using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Domain.WhatsAppAccounts;

public sealed class PhoneNumber
{
    public PhoneNumberId Id { get; }
    public WhatsAppAccountId WhatsAppAccountId { get; }
    public ExternalPhoneNumberId ExternalPhoneNumberId { get; }
    public string DisplayPhoneNumber { get; }
    public string? VerifiedName { get; }
    public PhoneNumberStatus Status { get; }
    public DateTimeOffset CreatedAt { get; }

    private PhoneNumber(PhoneNumberId id, WhatsAppAccountId whatsAppAccountId, ExternalPhoneNumberId externalPhoneNumberId,
        string displayPhoneNumber, string? verifiedName, PhoneNumberStatus status, DateTimeOffset createdAt)
    {
        Id = id;
        WhatsAppAccountId = whatsAppAccountId;
        ExternalPhoneNumberId = externalPhoneNumberId;
        DisplayPhoneNumber = displayPhoneNumber;
        VerifiedName = verifiedName;
        Status = status;
        CreatedAt = createdAt;
    }

    public static bool TryCreate(PhoneNumberId id, WhatsAppAccountId accountId, ExternalPhoneNumberId externalId,
        string? displayPhoneNumber, string? verifiedName, DateTimeOffset createdAt, out PhoneNumber? phone)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(accountId);
        ArgumentNullException.ThrowIfNull(externalId);
        phone = null;
        var display = displayPhoneNumber?.Trim();
        var name = string.IsNullOrWhiteSpace(verifiedName) ? null : verifiedName.Trim();
        if (string.IsNullOrWhiteSpace(display) || display.Length > 50 || display.Contains('\0') ||
            (name is not null && (name.Length > 200 || name.Contains('\0'))) || createdAt.Offset != TimeSpan.Zero) return false;
        phone = new(id, accountId, externalId, display, name, PhoneNumberStatus.Registered, createdAt);
        return true;
    }
}

using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Domain.WhatsAppAccounts;

public sealed class MessagingAccount
{
    public MessagingAccountId Id { get; }
    public WhatsAppAccountId WhatsAppAccountId { get; }
    public ExternalMessagingAccountId ExternalMessagingAccountId { get; }
    public DateTimeOffset CreatedAt { get; }

    private MessagingAccount(MessagingAccountId id, WhatsAppAccountId whatsAppAccountId,
        ExternalMessagingAccountId externalMessagingAccountId, DateTimeOffset createdAt)
    {
        Id = id;
        WhatsAppAccountId = whatsAppAccountId;
        ExternalMessagingAccountId = externalMessagingAccountId;
        CreatedAt = createdAt;
    }

    public static bool TryCreate(MessagingAccountId id, WhatsAppAccountId accountId,
        ExternalMessagingAccountId externalId, DateTimeOffset createdAt, out MessagingAccount? account)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(accountId);
        ArgumentNullException.ThrowIfNull(externalId);
        account = null;
        if (createdAt.Offset != TimeSpan.Zero) return false;
        account = new(id, accountId, externalId, createdAt);
        return true;
    }
}

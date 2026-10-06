using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Domain.WhatsAppAccounts;

public sealed class WhatsAppAccount
{
    public WhatsAppAccountId Id { get; }
    public OrganizationId OrganizationId { get; }
    public EmbeddedSignupSessionId SignupSessionId { get; }
    public ExternalWhatsAppAccountId ExternalWhatsAppAccountId { get; }
    public string DisplayName { get; }
    public WhatsAppAccountStatus Status { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ConnectedAt { get; }

    private WhatsAppAccount(WhatsAppAccountId id, OrganizationId organizationId, EmbeddedSignupSessionId signupSessionId,
        ExternalWhatsAppAccountId externalWhatsAppAccountId, string displayName, WhatsAppAccountStatus status,
        DateTimeOffset createdAt, DateTimeOffset? connectedAt)
    {
        Id = id;
        OrganizationId = organizationId;
        SignupSessionId = signupSessionId;
        ExternalWhatsAppAccountId = externalWhatsAppAccountId;
        DisplayName = displayName;
        Status = status;
        CreatedAt = createdAt;
        ConnectedAt = connectedAt;
    }

    public static bool TryCreateConnected(WhatsAppAccountId id, OrganizationId organizationId,
        EmbeddedSignupSessionId signupSessionId, ExternalWhatsAppAccountId externalId, string? displayName,
        DateTimeOffset connectedAt, out WhatsAppAccount? account)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(organizationId);
        ArgumentNullException.ThrowIfNull(signupSessionId);
        ArgumentNullException.ThrowIfNull(externalId);
        account = null;
        var name = NormalizeDisplayName(displayName, externalId);
        if (name.Length > 200 || name.Contains('\0') || connectedAt.Offset != TimeSpan.Zero) return false;
        account = new(id, organizationId, signupSessionId, externalId, name, WhatsAppAccountStatus.Connected, connectedAt, connectedAt);
        return true;
    }

    public static string NormalizeDisplayName(string? displayName, ExternalWhatsAppAccountId externalId) =>
        string.IsNullOrWhiteSpace(displayName) ? externalId.Value
        : string.Join(' ', displayName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Infrastructure.Meta.Credentials;

internal sealed class MetaCredentialRecord
{
    public Guid Id { get; set; }
    public EmbeddedSignupSessionId SessionId { get; set; } = null!;
    // Null is an exchange reservation, never a plaintext token or authorization code.
    public string? ProtectedCredential { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

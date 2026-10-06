using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class WhatsAppAccountTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly WhatsAppAccountId Id = new(Guid.Parse("bd20d74a-9d82-4eba-a81e-a7c1f058a3d6"));
    private static readonly OrganizationId OrganizationId = new(Guid.Parse("ad20d74a-9d82-4eba-a81e-a7c1f058a3d6"));
    private static readonly EmbeddedSignupSessionId SessionId = new(Guid.Parse("cd20d74a-9d82-4eba-a81e-a7c1f058a3d6"));

    private static ExternalWhatsAppAccountId ExternalId()
    {
        Assert.True(ExternalWhatsAppAccountId.TryCreate("123456", out var id));
        return Assert.IsType<ExternalWhatsAppAccountId>(id);
    }

    [Fact]
    public void Connected_account_has_internal_identity_ownership_and_utc_connection_time()
    {
        Assert.True(WhatsAppAccount.TryCreateConnected(Id, OrganizationId, SessionId, ExternalId(), "  Example\t account ", Now, out var account));
        Assert.NotNull(account);
        Assert.Equal(Id, account.Id);
        Assert.Equal(OrganizationId, account.OrganizationId);
        Assert.Equal("Example account", account.DisplayName);
        Assert.Equal(WhatsAppAccountStatus.Connected, account.Status);
        Assert.Equal(Now, account.ConnectedAt);
        Assert.True(WhatsAppAccount.TryCreateConnected(Id, OrganizationId, SessionId, ExternalId(), null, Now, out var unnamed));
        Assert.Equal("123456", Assert.IsType<WhatsAppAccount>(unnamed).DisplayName);
    }

    [Fact]
    public void Invalid_account_name_or_non_utc_time_is_rejected()
    {
        Assert.False(WhatsAppAccount.TryCreateConnected(Id, OrganizationId, SessionId, ExternalId(), new string('x', 201), Now, out _));
        Assert.False(WhatsAppAccount.TryCreateConnected(Id, OrganizationId, SessionId, ExternalId(), "bad\0name", Now, out _));
        Assert.False(WhatsAppAccount.TryCreateConnected(Id, OrganizationId, SessionId, ExternalId(), "Example", Now.ToOffset(TimeSpan.FromHours(1)), out _));
    }

    [Fact]
    public void External_identifiers_preserve_opaque_provider_values()
    {
        foreach (var value in new[] { "provider:001/a-B", "0123", "0", "référence-😀", new string('x', 100), string.Concat(Enumerable.Repeat("😀", 100)) })
        {
            Assert.True(ExternalWhatsAppAccountId.TryCreate(value, out var accountId));
            Assert.True(ExternalMessagingAccountId.TryCreate(value, out var messagingId));
            Assert.True(ExternalPhoneNumberId.TryCreate(value, out var phoneId));
            Assert.Equal(value, accountId?.Value);
            Assert.Equal(value, messagingId?.Value);
            Assert.Equal(value, phoneId?.Value);
        }
    }

    [Fact]
    public void External_identifiers_reject_missing_oversized_control_or_invalid_unicode_values()
    {
        foreach (var value in new[] { null, "", "   ", new string('x', 101), string.Concat(Enumerable.Repeat("😀", 101)), "id\0", "id\n", "id\u007f", "id\u0085", "id\ud800" })
        {
            Assert.False(ExternalWhatsAppAccountId.TryCreate(value, out _));
            Assert.False(ExternalMessagingAccountId.TryCreate(value, out _));
            Assert.False(ExternalPhoneNumberId.TryCreate(value, out _));
        }
    }
}

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
    public void External_identifiers_reject_malformed_values_instead_of_becoming_internal_ids()
    {
        Assert.False(ExternalWhatsAppAccountId.TryCreate("not-an-id", out _));
        Assert.False(ExternalMessagingAccountId.TryCreate("0123", out _));
        Assert.False(ExternalPhoneNumberId.TryCreate("", out _));
        Assert.False(ExternalPhoneNumberId.TryCreate(new string('1', 101), out _));
        Assert.False(ExternalWhatsAppAccountId.TryCreate(" 123 ", out _));
    }
}

using System.Net;
using System.Text.Json;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;
using WhatsAppPlatform.Infrastructure.Meta;
using WhatsAppPlatform.Infrastructure.Meta.EmbeddedSignup;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class MetaEmbeddedSignupTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);
    private const string ValidDebug = """
        {"data":{"app_id":"test-app","is_valid":true,"expires_at":0,
        "scopes":["whatsapp_business_management","whatsapp_business_messaging"],
        "granular_scopes":[{"scope":"whatsapp_business_management","target_ids":["provider-account"]}]}}
        """;

    [Fact]
    public void Discovery_uses_one_explicitly_authorized_account_and_fails_closed()
    {
        using var valid = JsonDocument.Parse(ValidDebug);
        Assert.Equal("provider-account", MetaTokenValidation.GetSingleAuthorizedAccount(valid.RootElement, "test-app", Now));
        Assert.Null(MetaTokenValidation.GetSingleAuthorizedAccount(valid.RootElement, "other-app", Now));
        foreach (var invalid in new[] {
            ValidDebug.Replace("true", "false", StringComparison.Ordinal),
            ValidDebug.Replace("[\"provider-account\"]", "[\"a\",\"b\"]", StringComparison.Ordinal),
            ValidDebug.Replace("[\"provider-account\"]", "[]", StringComparison.Ordinal),
            ValidDebug.Replace("whatsapp_business_messaging", "unrelated_permission", StringComparison.Ordinal),
            ValidDebug.Replace("\"expires_at\":0", "\"expires_at\":1", StringComparison.Ordinal),
            ValidDebug.Replace("\"expires_at\":0", "\"expires_at\":\"malformed\"", StringComparison.Ordinal) })
        {
            using var document = JsonDocument.Parse(invalid);
            Assert.Null(MetaTokenValidation.GetSingleAuthorizedAccount(document.RootElement, "test-app", Now));
        }
    }

    [Fact]
    public void Credential_checkpoint_prevents_reexchange_on_replay_or_uncertain_exchange()
    {
        Assert.Equal(MetaExchangeDecision.ExchangeOnce, MetaCredentialReplay.Decide(null));
        Assert.Equal(MetaExchangeDecision.RestartRequired, MetaCredentialReplay.Decide(new(null)));
        Assert.Equal(MetaExchangeDecision.DiscoverWithStoredCredential,
            MetaCredentialReplay.Decide(new(new MetaCredential("synthetic-test-value"))));
    }

    [Fact]
    public void Credential_cannot_leak_through_serialization_or_string_formatting()
    {
        var credential = new MetaCredential("synthetic-test-value");
        Assert.Equal("{}", JsonSerializer.Serialize(credential));
        Assert.DoesNotContain(credential.AccessToken, credential.ToString(), StringComparison.Ordinal);
        var options = new MetaOptions { AppSecret = "synthetic-test-secret" };
        Assert.DoesNotContain(options.AppSecret, JsonSerializer.Serialize(options), StringComparison.Ordinal);
        Assert.DoesNotContain(options.AppSecret, options.ToString(), StringComparison.Ordinal);
        using var publicJson = JsonDocument.Parse(JsonSerializer.Serialize(
            new MetaSignupConfigurationResponse(true, "app", "configuration", "v25.0"), JsonSerializerOptions.Web));
        Assert.Equal(new[] { "enabled", "appId", "configurationId", "graphApiVersion" },
            publicJson.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, MetaGatewayError.Rejected)]
    [InlineData(HttpStatusCode.Unauthorized, MetaGatewayError.Rejected)]
    [InlineData(HttpStatusCode.TooManyRequests, MetaGatewayError.Unavailable)]
    [InlineData(HttpStatusCode.BadGateway, MetaGatewayError.Unavailable)]
    public void Provider_errors_are_classified_without_exposing_provider_content(HttpStatusCode status, MetaGatewayError expected)
        => Assert.Equal(expected, MetaGraphClient.Classify(status));
}

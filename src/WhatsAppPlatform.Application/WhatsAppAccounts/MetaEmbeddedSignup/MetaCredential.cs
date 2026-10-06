using System.Text.Json.Serialization;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

// Secret-bearing infrastructure/application contract. Never a response or domain entity.
public sealed class MetaCredential
{
    [JsonIgnore] public string AccessToken { get; }
    public MetaCredential(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        AccessToken = accessToken;
    }
    public override string ToString() => "[Protected Meta credential]";
}

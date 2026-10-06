using System.Text.Json.Serialization;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MetaCompletionRequest
{
    public string? AuthorizationCode { get; init; }
    public override string ToString() => "[Meta authorization result]";
}

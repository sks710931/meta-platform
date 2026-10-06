using System.Text.Json;
using Microsoft.Extensions.Options;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;
using WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Infrastructure.Meta.EmbeddedSignup;

internal sealed class MetaEmbeddedSignupGateway(MetaGraphClient client, IOptions<MetaOptions> options, TimeProvider clock) : IMetaEmbeddedSignupGateway
{
    public bool IsEnabled => options.Value.IsEnabled;
    public MetaSignupConfigurationResponse GetPublicConfiguration() => IsEnabled
        ? new(true, options.Value.AppId, options.Value.EmbeddedSignupConfigurationId, options.Value.GraphApiVersion)
        : new(false, null, null, null);
    public async Task<MetaGatewayResult<MetaCredential>> ExchangeAsync(string authorizationCode, CancellationToken cancellationToken)
    {
        var config = options.Value;
        var result = await client.GetAsync($"oauth/access_token?client_id={Uri.EscapeDataString(config.AppId)}&client_secret={Uri.EscapeDataString(config.AppSecret)}&code={Uri.EscapeDataString(authorizationCode)}", null, cancellationToken);
        using var document = result.Value;
        if (document is null) return MetaGatewayResult<MetaCredential>.Failure(result.Error ?? MetaGatewayError.Rejected);
        var token = MetaTokenValidation.Text(document.RootElement, "access_token");
        if (string.IsNullOrWhiteSpace(token) || token.Length > 16384 || token.Any(character => character is < '!' or > '~'))
            return MetaGatewayResult<MetaCredential>.Failure(MetaGatewayError.Rejected);
        return MetaGatewayResult<MetaCredential>.Success(new MetaCredential(token));
    }

    public async Task<MetaGatewayResult<RegisterOnboardingInput>> DiscoverAsync(MetaCredential credential, CancellationToken cancellationToken)
    {
        var config = options.Value;
        var debug = await client.GetAsync($"debug_token?input_token={Uri.EscapeDataString(credential.AccessToken)}",
            $"{config.AppId}|{config.AppSecret}", cancellationToken);
        using var debugDocument = debug.Value;
        if (debugDocument is null) return Failure(debug.Error);
        var accountId = MetaTokenValidation.GetSingleAuthorizedAccount(debugDocument.RootElement, config.AppId, clock.GetUtcNow());
        if (!ExternalMessagingAccountId.TryCreate(accountId, out _) || accountId is null)
            return Failure(MetaGatewayError.UnsupportedAssets);
        var account = await client.GetAsync($"{Uri.EscapeDataString(accountId)}?fields=id,name", credential.AccessToken, cancellationToken);
        using var accountDocument = account.Value;
        if (accountDocument is null) return Failure(account.Error);
        if (MetaTokenValidation.Text(accountDocument.RootElement, "id") != accountId) return Failure(MetaGatewayError.Rejected);
        var name = MetaTokenValidation.Text(accountDocument.RootElement, "name");
        var phones = await GetPhonesAsync(accountId, credential, cancellationToken);
        if (phones.Value is null) return Failure(phones.Error);
        // Phase 1: WABA field identifies Messaging Account; Meta does not expose the separate WAAC ID.
        return MetaGatewayResult<RegisterOnboardingInput>.Success(new(null, accountId,
            string.IsNullOrWhiteSpace(name) ? accountId : name, phones.Value));
    }

    private async Task<MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>> GetPhonesAsync(string accountId,
        MetaCredential credential, CancellationToken cancellationToken)
    {
        var phones = new List<RegisterPhoneInput?>();
        string? after = null;
        for (var page = 0; page < 10; page++)
        {
            var result = await client.GetAsync($"{Uri.EscapeDataString(accountId)}/phone_numbers?fields=id,display_phone_number,verified_name&limit=100" +
                (after is null ? "" : $"&after={Uri.EscapeDataString(after)}"), credential.AccessToken, cancellationToken);
            using var document = result.Value;
            if (document is null) return MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>.Failure(result.Error ?? MetaGatewayError.Rejected);
            var root = document.RootElement;
            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                return MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>.Failure(MetaGatewayError.Rejected);
            foreach (var phone in data.EnumerateArray())
            {
                if (phone.ValueKind != JsonValueKind.Object) return MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>.Failure(MetaGatewayError.Rejected);
                phones.Add(new(MetaTokenValidation.Text(phone, "id"), MetaTokenValidation.Text(phone, "display_phone_number"), MetaTokenValidation.Text(phone, "verified_name")));
            }
            if (phones.Count > 100) break;
            if (!root.TryGetProperty("paging", out var paging) || paging.ValueKind != JsonValueKind.Object || !paging.TryGetProperty("next", out _))
                return phones.Count > 0 ? MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>.Success(phones)
                    : MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>.Failure(MetaGatewayError.UnsupportedAssets);
            if (!paging.TryGetProperty("cursors", out var cursors) || cursors.ValueKind != JsonValueKind.Object ||
                MetaTokenValidation.Text(cursors, "after") is not { Length: > 0 and <= 4096 } cursor || cursor == after) break;
            after = cursor;
            // Never follow a provider-supplied URL: it could contain a token or change the trusted host.
        }
        return MetaGatewayResult<IReadOnlyList<RegisterPhoneInput?>>.Failure(MetaGatewayError.UnsupportedAssets);
    }

    private static MetaGatewayResult<RegisterOnboardingInput> Failure(MetaGatewayError? error) =>
        MetaGatewayResult<RegisterOnboardingInput>.Failure(error ?? MetaGatewayError.Rejected);
}

using System.Text.Json;

namespace WhatsAppPlatform.Infrastructure.Meta.EmbeddedSignup;

public static class MetaTokenValidation
{
    public static string? GetSingleAuthorizedAccount(JsonElement root, string appId, DateTimeOffset now)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
            Text(data, "app_id") != appId || !data.TryGetProperty("is_valid", out var valid) || valid.ValueKind != JsonValueKind.True ||
            !data.TryGetProperty("scopes", out var scopes) || scopes.ValueKind != JsonValueKind.Array ||
            !new[] { "whatsapp_business_management", "whatsapp_business_messaging" }.All(scope => scopes.EnumerateArray().Any(value => value.ValueKind == JsonValueKind.String && value.GetString() == scope)) ||
            IsExpired(data, "expires_at", now) || IsExpired(data, "data_access_expires_at", now) ||
            !data.TryGetProperty("granular_scopes", out var granular) || granular.ValueKind != JsonValueKind.Array) return null;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var permission in granular.EnumerateArray())
        {
            if (permission.ValueKind != JsonValueKind.Object || Text(permission, "scope") != "whatsapp_business_management") continue;
            if (!permission.TryGetProperty("target_ids", out var targets) || targets.ValueKind != JsonValueKind.Array) return null;
            foreach (var target in targets.EnumerateArray())
            {
                var id = target.ValueKind == JsonValueKind.String ? target.GetString()
                    : target.ValueKind == JsonValueKind.Number && target.TryGetInt64(out var numeric) ? numeric.ToString(System.Globalization.CultureInfo.InvariantCulture) : null;
                if (string.IsNullOrWhiteSpace(id)) return null;
                ids.Add(id);
            }
        }
        // Fail closed for unrestricted/multiple assets; no browser hint chooses an arbitrary resource.
        return ids.Count == 1 ? ids.Single() : null;
    }

    public static string? Text(JsonElement value, string name) => value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() : null;
    private static bool IsExpired(JsonElement data, string name, DateTimeOffset now) =>
        data.TryGetProperty(name, out var property) && (property.ValueKind != JsonValueKind.Number || !property.TryGetInt64(out var timestamp) || timestamp < 0 ||
            timestamp > 0 && timestamp <= now.ToUnixTimeSeconds());
}

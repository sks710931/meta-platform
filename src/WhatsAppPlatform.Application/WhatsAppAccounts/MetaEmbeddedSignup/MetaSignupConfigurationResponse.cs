namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

// Explicit public allowlist. The secret configuration type stays in Infrastructure.
public sealed record MetaSignupConfigurationResponse(bool Enabled, string? AppId,
    string? ConfigurationId, string? GraphApiVersion);

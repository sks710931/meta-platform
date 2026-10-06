namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

public enum MetaExchangeDecision { ExchangeOnce, DiscoverWithStoredCredential, RestartRequired }
public static class MetaCredentialReplay
{
    public static MetaExchangeDecision Decide(StoredMetaCredential? stored) => stored switch
    {
        null => MetaExchangeDecision.ExchangeOnce,
        { Credential: null } => MetaExchangeDecision.RestartRequired,
        _ => MetaExchangeDecision.DiscoverWithStoredCredential
    };
}

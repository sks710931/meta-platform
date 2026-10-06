using WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

public interface IMetaEmbeddedSignupGateway
{
    bool IsEnabled { get; }
    MetaSignupConfigurationResponse GetPublicConfiguration();
    Task<MetaGatewayResult<MetaCredential>> ExchangeAsync(string authorizationCode, CancellationToken cancellationToken);
    Task<MetaGatewayResult<RegisterOnboardingInput>> DiscoverAsync(MetaCredential credential, CancellationToken cancellationToken);
}

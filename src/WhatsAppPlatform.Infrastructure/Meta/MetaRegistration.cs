using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;
using WhatsAppPlatform.Infrastructure.Meta.Credentials;
using WhatsAppPlatform.Infrastructure.Meta.EmbeddedSignup;

namespace WhatsAppPlatform.Infrastructure.Meta;

public static class MetaRegistration
{
    public static IServiceCollection AddMetaEmbeddedSignup(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<MetaOptions>, MetaOptionsValidator>();
        services.AddOptions<MetaOptions>().Bind(configuration.GetSection("Meta")).ValidateOnStart();
        // Exchange/debug query parameters are secrets. Remove HttpClientFactory URL/header logging.
        services.AddHttpClient<MetaGraphClient>(client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.MaxResponseContentBufferSize = 1024 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
          .RemoveAllLoggers();
        services.AddScoped<IMetaEmbeddedSignupGateway, MetaEmbeddedSignupGateway>();
        services.AddScoped<IMetaCredentialStore, ProtectedMetaCredentialStore>();
        services.AddScoped<IOnboardingAttemptCoordinator, PostgresOnboardingAttemptCoordinator>();
        services.AddScoped<CompleteMetaOnboardingHandler>();
        return services;
    }
}

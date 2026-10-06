using WhatsAppPlatform.Application.WhatsAppAccounts.GetOnboardingSession;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class GetOnboardingSessionEndpoint
{
    public static void MapGetOnboardingSession(this WebApplication app) =>
        app.MapGet("/api/whatsapp/onboarding-sessions/{sessionId}", HandleAsync);

    private static async Task<IResult> HandleAsync(string sessionId, GetOnboardingSessionHandler handler,
        IHostEnvironment environment, CancellationToken cancellationToken)
    {
        if (!OnboardingHttpResults.TryIdentifier(sessionId, out var value)) return OnboardingHttpResults.InvalidIdentifier("sessionId");
        var result = await handler.HandleAsync(new EmbeddedSignupSessionId(value), cancellationToken);
        return result.Value is null ? OnboardingHttpResults.Error(result.Error, result.Message)
            : Results.Ok(OnboardingSessionView.FromSession(result.Value, environment));
    }
}

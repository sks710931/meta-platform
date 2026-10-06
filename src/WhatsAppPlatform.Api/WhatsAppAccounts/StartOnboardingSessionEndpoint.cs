using WhatsAppPlatform.Application.WhatsAppAccounts.StartOnboardingSession;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class StartOnboardingSessionEndpoint
{
    public static void MapStartOnboardingSession(this WebApplication app) =>
        app.MapPost("/api/organizations/{organizationId}/whatsapp/onboarding-sessions", HandleAsync);

    private static async Task<IResult> HandleAsync(string organizationId, StartOnboardingSessionHandler handler,
        IHostEnvironment environment, CancellationToken cancellationToken)
    {
        if (!OnboardingHttpResults.TryIdentifier(organizationId, out var value)) return OnboardingHttpResults.InvalidIdentifier("organizationId");
        var result = await handler.HandleAsync(new OrganizationId(value), cancellationToken);
        return result.Value is null ? OnboardingHttpResults.Error(result.Error, result.Message)
            : Results.Created($"/api/whatsapp/onboarding-sessions/{result.Value.SessionId}", OnboardingSessionView.FromSession(result.Value, environment));
    }
}

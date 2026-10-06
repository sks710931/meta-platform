using WhatsAppPlatform.Api.Identity;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class CompleteMetaOnboardingEndpoint
{
    public static void MapMetaCompletion(this WebApplication app) =>
        app.MapPost("/api/whatsapp/onboarding-sessions/{sessionId}/meta-complete", HandleAsync)
            .RequireAuthorization().AddEndpointFilter<OrganizationAccessFilter>()
            .WithMetadata(new OrganizationAdministrationAccess());

    private static async Task<IResult> HandleAsync(string sessionId, MetaCompletionRequest request,
        CompleteMetaOnboardingHandler handler, CancellationToken cancellationToken)
    {
        if (!OnboardingHttpResults.TryIdentifier(sessionId, out var id)) return OnboardingHttpResults.InvalidIdentifier("sessionId");
        var result = await handler.HandleAsync(new EmbeddedSignupSessionId(id), request.AuthorizationCode, cancellationToken);
        if (result.Value is null) return OnboardingHttpResults.Error(result.Error, result.Message);
        return result.Value.AlreadyCompleted ? Results.Ok(result.Value)
            : Results.Created($"/api/whatsapp-accounts/{result.Value.Account.WhatsAppAccountId}", result.Value);
    }
}

using WhatsAppPlatform.Api.Identity;
using WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class CompleteOnboardingEndpoint
{
    public static void MapDevelopmentOnlyCompletion(this WebApplication app)
    {
        // TEMPORARY SCAFFOLDING: no completion route exists outside Development.
        // Replace with a trusted, validated Embedded Signup adapter when real integration is authorized.
        if (app.Environment.IsDevelopment())
            app.MapPost("/api/whatsapp/onboarding-sessions/{sessionId}/complete", HandleAsync).RequireAuthorization().AddEndpointFilter<OrganizationAccessFilter>().WithMetadata(new OrganizationAdministrationAccess());
    }

    private static async Task<IResult> HandleAsync(string sessionId, CompleteOnboardingRequest request,
        RegisterOnboardingResultHandler handler, CancellationToken cancellationToken)
    {
        if (!OnboardingHttpResults.TryIdentifier(sessionId, out var value)) return OnboardingHttpResults.InvalidIdentifier("sessionId");
        if (request.ExternalWhatsAppAccountId is null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["externalWhatsAppAccountId"] = ["External account identifier is required for manual completion."] });
        var result = await handler.HandleAsync(new EmbeddedSignupSessionId(value), request.ToInput(), cancellationToken);
        if (result.Value is null) return OnboardingHttpResults.Error(result.Error, result.Message);
        return result.Value.AlreadyCompleted ? Results.Ok(result.Value)
            : Results.Created($"/api/whatsapp-accounts/{result.Value.Account.WhatsAppAccountId}", result.Value);
    }
}

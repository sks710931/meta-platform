using WhatsAppPlatform.Api.Identity;
using WhatsAppPlatform.Application.WhatsAppAccounts.ListWhatsAppAccounts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class ListWhatsAppAccountsEndpoint
{
    public static void MapListWhatsAppAccounts(this WebApplication app) =>
        app.MapGet("/api/organizations/{organizationId}/whatsapp-accounts", HandleAsync).RequireAuthorization().AddEndpointFilter<OrganizationAccessFilter>();

    private static async Task<IResult> HandleAsync(string organizationId, ListWhatsAppAccountsHandler handler, CancellationToken cancellationToken)
    {
        if (!OnboardingHttpResults.TryIdentifier(organizationId, out var value)) return OnboardingHttpResults.InvalidIdentifier("organizationId");
        var result = await handler.HandleAsync(new OrganizationId(value), cancellationToken);
        return result.Value is null ? OnboardingHttpResults.Error(result.Error, result.Message) : Results.Ok(result.Value);
    }
}

using WhatsAppPlatform.Application.WhatsAppAccounts.GetWhatsAppAccount;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class GetWhatsAppAccountEndpoint
{
    public static void MapGetWhatsAppAccount(this WebApplication app) =>
        app.MapGet("/api/whatsapp-accounts/{whatsAppAccountId}", HandleAsync);

    private static async Task<IResult> HandleAsync(string whatsAppAccountId, GetWhatsAppAccountHandler handler, CancellationToken cancellationToken)
    {
        if (!OnboardingHttpResults.TryIdentifier(whatsAppAccountId, out var value)) return OnboardingHttpResults.InvalidIdentifier("whatsAppAccountId");
        var result = await handler.HandleAsync(new WhatsAppAccountId(value), cancellationToken);
        return result.Value is null ? OnboardingHttpResults.Error(result.Error, result.Message) : Results.Ok(result.Value);
    }
}

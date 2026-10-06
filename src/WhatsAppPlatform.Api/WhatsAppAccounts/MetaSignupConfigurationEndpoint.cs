using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class MetaSignupConfigurationEndpoint
{
    public static void MapMetaSignupConfiguration(this WebApplication app) =>
        app.MapGet("/api/whatsapp/embedded-signup/configuration", (IMetaEmbeddedSignupGateway gateway, HttpContext http) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            return Results.Ok(gateway.GetPublicConfiguration());
        }).RequireAuthorization();
}

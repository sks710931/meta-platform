using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using WhatsAppPlatform.Application.Identity.Contracts;

namespace WhatsAppPlatform.Api.Identity;

internal static class AuthenticationEndpoints
{
    public static void MapAuthentication(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth");
        auth.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(new { requestToken = antiforgery.GetAndStoreTokens(context).RequestToken });
        }).AllowAnonymous();
        auth.MapPost("/login", LoginAsync).AllowAnonymous().RequireRateLimiting("login");
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();
        auth.MapGet("/me", MeAsync).RequireAuthorization();
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, CookieIdentitySession session,
        HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 256 ||
            string.IsNullOrEmpty(request.Password) || request.Password.Length > 1024)
            return Results.Problem(statusCode: 400, title: "Provide a valid email and password.");
        return await session.LoginAsync(request.Email, request.Password, cancellationToken)
            ? Results.NoContent() : Results.Problem(statusCode: 401, title: "Invalid email or password.");
    }

    private static async Task<IResult> LogoutAsync(CookieIdentitySession session, HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-store";
        await session.LogoutAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> MeAsync(ICurrentUser user, IMembershipStore memberships,
        HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (user.UserId is not { } userId) return Results.Unauthorized();
        return Results.Ok(new { userId = userId.Value, user.Email, user.IsPlatformAdmin,
            organizations = await memberships.ListOrganizationsAsync(userId, cancellationToken) });
    }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record LoginRequest(string? Email, string? Password);

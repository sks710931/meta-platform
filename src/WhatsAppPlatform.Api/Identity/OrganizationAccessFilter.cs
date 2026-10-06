using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.Identity;

internal sealed record OrganizationAdministrationAccess;

internal sealed class OrganizationAccessFilter(IOrganizationAccessService access, ITenantResourceOwnership ownership) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var organizationId = await ResolveOrganizationAsync(http, http.RequestAborted);
        // Malformed IDs are handled by the existing endpoint validators; valid unknown resources return 404.
        if (organizationId is null)
            return HasMalformedId(http) ? await next(context) : Results.NotFound();
        if (!await access.CanAccessAsync(organizationId, http.RequestAborted)) return Results.NotFound();
        if (http.GetEndpoint()?.Metadata.GetMetadata<OrganizationAdministrationAccess>() is not null &&
            !await access.CanAdministerAsync(organizationId, http.RequestAborted)) return Results.Forbid();
        return await next(context);
    }

    private async Task<OrganizationId?> ResolveOrganizationAsync(HttpContext http, CancellationToken cancellationToken)
    {
        if (TryId(http, "organizationId", out var organization)) return new OrganizationId(organization);
        if (TryId(http, "sessionId", out var session))
            return await ownership.SessionOrganizationAsync(new EmbeddedSignupSessionId(session), cancellationToken);
        if (TryId(http, "whatsAppAccountId", out var account))
            return await ownership.AccountOrganizationAsync(new WhatsAppAccountId(account), cancellationToken);
        return null;
    }

    private static bool HasMalformedId(HttpContext http) =>
        new[] { "organizationId", "sessionId", "whatsAppAccountId" }.Any(key =>
            http.Request.RouteValues.ContainsKey(key) && !TryId(http, key, out _));

    private static bool TryId(HttpContext http, string key, out Guid id) =>
        Guid.TryParse(http.Request.RouteValues[key]?.ToString(), out id) && id != Guid.Empty;
}

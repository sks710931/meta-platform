using WhatsAppPlatform.Application.Organizations.GetOrganizationDetails;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Api.Organizations;

internal static class GetOrganizationDetailsEndpoint
{
    public static void MapGetOrganizationDetails(this RouteGroupBuilder group) =>
        group.MapGet("/{organizationId}", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string organizationId, GetOrganizationDetailsHandler handler, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(organizationId, out var value) || value == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["organizationId"] = ["Organization ID must be a nonempty UUID."]
            });
        }

        var result = await handler.HandleAsync(new OrganizationId(value), cancellationToken);
        return result switch
        {
            GetOrganizationDetailsResult.Found found => Results.Ok(found.Organization),
            GetOrganizationDetailsResult.NotFound => Results.NotFound(),
            _ => throw new InvalidOperationException("Unknown organization details result.")
        };
    }
}

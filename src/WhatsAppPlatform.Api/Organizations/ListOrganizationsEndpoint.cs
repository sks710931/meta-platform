using WhatsAppPlatform.Application.Organizations.ListOrganizations;

namespace WhatsAppPlatform.Api.Organizations;

internal static class ListOrganizationsEndpoint
{
    public static void MapListOrganizations(this RouteGroupBuilder group) => group.MapGet("", HandleAsync);

    private static async Task<IResult> HandleAsync(
        ListOrganizationsHandler handler, CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(cancellationToken));
}

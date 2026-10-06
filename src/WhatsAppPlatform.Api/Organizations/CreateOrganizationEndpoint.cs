using WhatsAppPlatform.Application.Organizations.CreateOrganization;

namespace WhatsAppPlatform.Api.Organizations;

internal static class CreateOrganizationEndpoint
{
    public static void MapCreateOrganization(this RouteGroupBuilder group) => group.MapPost("", HandleAsync);

    private static async Task<IResult> HandleAsync(
        CreateOrganizationRequest request, CreateOrganizationHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.Name, cancellationToken);
        return result switch
        {
            CreateOrganizationResult.Created created => Results.Created(
                $"/api/organizations/{created.Organization.OrganizationId}", created.Organization),
            CreateOrganizationResult.Invalid invalid => Results.ValidationProblem(
                new Dictionary<string, string[]> { ["name"] = [invalid.Message] }),
            _ => throw new InvalidOperationException("Unknown organization creation result.")
        };
    }
}

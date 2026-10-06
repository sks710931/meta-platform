using WhatsAppPlatform.Domain.Identity.Contracts;

namespace WhatsAppPlatform.Application.Identity;

public static class OrganizationAccessDecision
{
    public static bool CanAccess(UserId? userId, bool isPlatformAdmin, OrganizationRole? role) =>
        userId is not null && (isPlatformAdmin || role is OrganizationRole.Member or OrganizationRole.OrganizationAdmin);

    public static bool CanAdminister(UserId? userId, bool isPlatformAdmin, OrganizationRole? role) =>
        userId is not null && (isPlatformAdmin || role == OrganizationRole.OrganizationAdmin);
}

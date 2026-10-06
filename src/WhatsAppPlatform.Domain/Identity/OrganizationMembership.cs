using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Domain.Identity;

public sealed class OrganizationMembership
{
    public OrganizationMembershipId Id { get; }
    public OrganizationId OrganizationId { get; }
    public UserId UserId { get; }
    public OrganizationRole Role { get; }
    public DateTimeOffset CreatedAt { get; }

    private OrganizationMembership(OrganizationMembershipId id, OrganizationId organizationId, UserId userId,
        OrganizationRole role, DateTimeOffset createdAt)
    {
        Id = id; OrganizationId = organizationId; UserId = userId; Role = role; CreatedAt = createdAt;
    }

    public static bool TryCreate(OrganizationMembershipId id, OrganizationId organizationId, UserId userId,
        OrganizationRole role, DateTimeOffset createdAt, out OrganizationMembership? membership)
    {
        membership = null;
        if (id is null || organizationId is null || userId is null || !Enum.IsDefined(role) || createdAt.Offset != TimeSpan.Zero)
            return false;
        membership = new(id, organizationId, userId, role, createdAt);
        return true;
    }
}

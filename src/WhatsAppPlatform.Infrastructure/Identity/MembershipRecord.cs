using WhatsAppPlatform.Domain.Identity;
using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Infrastructure.Identity;

internal sealed class MembershipRecord
{
    public Guid Id { get; private set; }
    public OrganizationId OrganizationId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public OrganizationRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private MembershipRecord() { }
    public static MembershipRecord FromDomain(OrganizationMembership membership) => new()
    {
        Id = membership.Id.Value, OrganizationId = membership.OrganizationId, UserId = membership.UserId.Value,
        Role = membership.Role, CreatedAt = membership.CreatedAt
    };
}

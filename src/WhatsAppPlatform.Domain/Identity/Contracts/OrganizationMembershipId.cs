namespace WhatsAppPlatform.Domain.Identity.Contracts;

public sealed record OrganizationMembershipId
{
    public Guid Value { get; }
    public OrganizationMembershipId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("An internal identifier must not be empty.", nameof(value));
        Value = value;
    }
}

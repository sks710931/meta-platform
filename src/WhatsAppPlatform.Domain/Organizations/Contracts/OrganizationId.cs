namespace WhatsAppPlatform.Domain.Organizations.Contracts;

public sealed record OrganizationId
{
    public Guid Value { get; }

    public OrganizationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("An internal identifier must not be empty.", nameof(value));
        }

        Value = value;
    }
}

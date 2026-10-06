namespace WhatsAppPlatform.Domain.Billing.Contracts;

public sealed record CreditLineAssignmentId
{
    public Guid Value { get; }

    public CreditLineAssignmentId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("An internal identifier must not be empty.", nameof(value));
        }

        Value = value;
    }
}

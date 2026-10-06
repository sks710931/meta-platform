using WhatsAppPlatform.Domain.Billing.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class AggregateIdTests
{
    [Fact]
    public void Internal_ids_reject_empty_identifiers()
    {
        Assert.Throws<ArgumentException>(() => new OrganizationId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new WhatsAppAccountId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new EmbeddedSignupSessionId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new MessagingAccountId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new PhoneNumberId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new CreditLineAssignmentId(Guid.Empty));
    }

    [Fact]
    public void Internal_ids_preserve_value_identity_without_cross_context_equality()
    {
        var value = Guid.Parse("01d02896-7a3b-4f64-b447-705ffbe0c61f");
        var organizationId = new OrganizationId(value);

        Assert.Equal(organizationId, new OrganizationId(value));
        Assert.NotEqual(organizationId, new OrganizationId(Guid.Parse("cd8be4a5-81df-41f5-9d5b-edc74c433aa7")));
        Assert.False(organizationId.Equals((object)new WhatsAppAccountId(value)));
    }
}

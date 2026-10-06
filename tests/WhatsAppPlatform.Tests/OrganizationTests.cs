using WhatsAppPlatform.Domain.Organizations;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class OrganizationTests
{
    private static readonly OrganizationId Id = new(Guid.Parse("bd20d74a-9d82-4eba-a81e-a7c1f058a3d6"));
    private static readonly DateTimeOffset CreatedAt = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Creation_normalizes_name_and_starts_active_with_supplied_utc_time()
    {
        var result = Organization.Create(Id, "  Example\t customer \n ", CreatedAt);
        var organization = Assert.IsType<OrganizationCreationResult.Created>(result).Organization;

        Assert.Equal("Example customer", organization.Name);
        Assert.Equal(OrganizationStatus.Active, organization.Status);
        Assert.Equal(CreatedAt, organization.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    [InlineData("bad\0name")]
    public void Creation_rejects_invalid_names(string? name)
    {
        Assert.IsType<OrganizationCreationResult.Invalid>(Organization.Create(Id, name, CreatedAt));
    }

    [Fact]
    public void Creation_rejects_names_exceeding_storage_limit()
    {
        Assert.IsType<OrganizationCreationResult.Invalid>(
            Organization.Create(Id, new string('x', Organization.MaximumNameLength + 1), CreatedAt));
    }

    [Fact]
    public void Creation_rejects_non_utc_timestamp()
    {
        Assert.IsType<OrganizationCreationResult.Invalid>(
            Organization.Create(Id, "Example", CreatedAt.ToOffset(TimeSpan.FromHours(2))));
    }
}

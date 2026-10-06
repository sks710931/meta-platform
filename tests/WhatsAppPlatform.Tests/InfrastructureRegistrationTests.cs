using Microsoft.Extensions.DependencyInjection;
using WhatsAppPlatform.Infrastructure;
using WhatsAppPlatform.Infrastructure.Persistence;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public void Registration_provides_a_scoped_postgresql_context()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure("Host=localhost;Database=registration_test;Username=test;Password=test");
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<PlatformDbContext>();

        Assert.Same(first, firstScope.ServiceProvider.GetRequiredService<PlatformDbContext>());
        Assert.NotSame(first, secondScope.ServiceProvider.GetRequiredService<PlatformDbContext>());
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", first.Database.ProviderName);
    }
}

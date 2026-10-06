using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WhatsAppPlatform.Domain.Identity;
using WhatsAppPlatform.Domain.Identity.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Identity;

public sealed class BootstrapIdentityProvisioner(UserManager<PlatformUser> users, RoleManager<IdentityRole<Guid>> roles,
    PlatformDbContext context, IConfiguration configuration, TimeProvider clock)
{
    public async Task ProvisionAsync(CancellationToken cancellationToken)
    {
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrEmpty(password)) return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Bootstrap admin configuration is incomplete.");
        var user = await EnsureUserAsync(email, password, cancellationToken);
        if (!await roles.RoleExistsAsync("PlatformAdmin"))
            EnsureSuccess(await roles.CreateAsync(new IdentityRole<Guid>("PlatformAdmin") { Id = Guid.NewGuid() }));
        if (!await users.IsInRoleAsync(user, "PlatformAdmin")) EnsureSuccess(await users.AddToRoleAsync(user, "PlatformAdmin"));
    }

    public async Task ProvisionDevelopmentMembershipAsync(CancellationToken cancellationToken)
    {
        var email = configuration["DevelopmentMember:Email"];
        var password = configuration["DevelopmentMember:Password"];
        var organizationValue = configuration["DevelopmentMember:OrganizationId"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrEmpty(password) && string.IsNullOrEmpty(organizationValue)) return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password) ||
            !Guid.TryParse(organizationValue, out var organizationId) || organizationId == Guid.Empty ||
            !Enum.TryParse<OrganizationRole>(configuration["DevelopmentMember:Role"], out var role) || !Enum.IsDefined(role))
            throw new InvalidOperationException("Development membership configuration is incomplete or invalid.");
        var organization = new OrganizationId(organizationId);
        if (!await context.Set<Domain.Organizations.Organization>().AnyAsync(row => row.Id == organization, cancellationToken))
            throw new InvalidOperationException("Development membership organization does not exist.");
        var user = await EnsureUserAsync(email, password, cancellationToken);
        if (await context.Set<MembershipRecord>().AnyAsync(row => row.OrganizationId == organization && row.UserId == user.Id, cancellationToken)) return;
        var now = clock.GetUtcNow();
        if (!OrganizationMembership.TryCreate(new OrganizationMembershipId(Guid.NewGuid()), organization, new UserId(user.Id), role,
            now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMillisecond)), out var membership) || membership is null)
            throw new InvalidOperationException("Development membership could not be created.");
        context.Set<MembershipRecord>().Add(MembershipRecord.FromDomain(membership));
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PlatformUser> EnsureUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = await users.FindByEmailAsync(email.Trim());
        if (existing is not null) return existing;
        var user = new PlatformUser { Id = Guid.NewGuid(), UserName = email.Trim(), Email = email.Trim(), LockoutEnabled = true };
        EnsureSuccess(await users.CreateAsync(user, password));
        return user;
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("Identity provisioning failed. Verify configuration and password policy.");
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using WhatsAppPlatform.Infrastructure.Identity;

namespace WhatsAppPlatform.Api.Identity;

// Composition adapter for framework cookie operations, never exposed to Domain/Application.
internal sealed class CookieIdentitySession(UserManager<PlatformUser> users, SignInManager<PlatformUser> signIn,
    UnknownUserPasswordCheck timing, TimeProvider clock)
{
    public async Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null) { timing.Verify(password); return false; }
        var result = await signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded || user.TwoFactorEnabled) return false;
        var deadline = clock.GetUtcNow().AddMinutes(30);
        var properties = new AuthenticationProperties { IsPersistent = false, ExpiresUtc = deadline };
        properties.Items["platform-session-deadline"] = deadline.ToString("O");
        await signIn.SignInAsync(user, properties);
        return true;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signIn.Context.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}

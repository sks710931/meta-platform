using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using WhatsAppPlatform.Infrastructure.Identity;

namespace WhatsAppPlatform.Api.Identity;

// Composition adapter: Identity framework types are confined to API/Infrastructure.
internal sealed class UnknownUserPasswordCheck
{
    private readonly PasswordHasher<PlatformUser> hasher = new();
    private readonly PlatformUser user = new();
    private readonly string hash;

    public UnknownUserPasswordCheck() => hash = hasher.HashPassword(user, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    public void Verify(string password) => hasher.VerifyHashedPassword(user, hash, password);
}

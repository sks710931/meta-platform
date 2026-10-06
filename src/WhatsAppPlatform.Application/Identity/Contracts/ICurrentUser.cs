using WhatsAppPlatform.Domain.Identity.Contracts;

namespace WhatsAppPlatform.Application.Identity.Contracts;

public interface ICurrentUser
{
    UserId? UserId { get; }
    string? Email { get; }
    bool IsPlatformAdmin { get; }
}

using System.Security.Claims;
using WhatsAppPlatform.Application.Identity.Contracts;
using WhatsAppPlatform.Domain.Identity.Contracts;

namespace WhatsAppPlatform.Api.Identity;

internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public UserId? UserId => accessor.HttpContext?.User.Identity?.IsAuthenticated == true &&
        Guid.TryParse(accessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id != Guid.Empty
        ? new UserId(id) : null;
    public string? Email => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    public bool IsPlatformAdmin => UserId is not null && accessor.HttpContext?.User.IsInRole("PlatformAdmin") == true;
}

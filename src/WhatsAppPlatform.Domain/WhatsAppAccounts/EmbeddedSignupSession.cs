using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Domain.WhatsAppAccounts;

public sealed class EmbeddedSignupSession
{
    public EmbeddedSignupSessionId Id { get; }
    public OrganizationId OrganizationId { get; }
    public EmbeddedSignupSessionStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private EmbeddedSignupSession(EmbeddedSignupSessionId id, OrganizationId organizationId,
        EmbeddedSignupSessionStatus status, DateTimeOffset startedAt, DateTimeOffset expiresAt, DateTimeOffset? completedAt)
    {
        Id = id;
        OrganizationId = organizationId;
        Status = status;
        StartedAt = startedAt;
        ExpiresAt = expiresAt;
        CompletedAt = completedAt;
    }

    public static bool TryStart(EmbeddedSignupSessionId id, OrganizationId organizationId,
        DateTimeOffset startedAt, DateTimeOffset expiresAt, out EmbeddedSignupSession? session)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(organizationId);
        session = null;
        if (startedAt.Offset != TimeSpan.Zero || expiresAt.Offset != TimeSpan.Zero || expiresAt <= startedAt)
            return false;
        session = new(id, organizationId, EmbeddedSignupSessionStatus.Pending, startedAt, expiresAt, null);
        return true;
    }

    public SignupTransition Complete(DateTimeOffset completedAt)
    {
        if (Status != EmbeddedSignupSessionStatus.Pending) return SignupTransition.NotPending;
        if (completedAt.Offset != TimeSpan.Zero || completedAt < StartedAt) return SignupTransition.InvalidTimestamp;
        if (ExpireIfDue(completedAt)) return SignupTransition.Expired;
        Status = EmbeddedSignupSessionStatus.Completed;
        CompletedAt = completedAt;
        return SignupTransition.Applied;
    }

    public SignupTransition Fail(DateTimeOffset failedAt)
    {
        if (Status != EmbeddedSignupSessionStatus.Pending) return SignupTransition.NotPending;
        if (failedAt.Offset != TimeSpan.Zero || failedAt < StartedAt) return SignupTransition.InvalidTimestamp;
        if (ExpireIfDue(failedAt)) return SignupTransition.Expired;
        Status = EmbeddedSignupSessionStatus.Failed;
        return SignupTransition.Applied;
    }

    public bool ExpireIfDue(DateTimeOffset now)
    {
        if (now.Offset != TimeSpan.Zero) throw new ArgumentException("Clock must be UTC.", nameof(now));
        if (Status != EmbeddedSignupSessionStatus.Pending || now < ExpiresAt) return false;
        Status = EmbeddedSignupSessionStatus.Expired;
        return true;
    }
}

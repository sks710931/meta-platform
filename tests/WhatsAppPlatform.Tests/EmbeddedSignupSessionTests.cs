using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using Xunit;

namespace WhatsAppPlatform.Tests;

public sealed class EmbeddedSignupSessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static EmbeddedSignupSession Session()
    {
        Assert.True(EmbeddedSignupSession.TryStart(
            new EmbeddedSignupSessionId(Guid.Parse("bd20d74a-9d82-4eba-a81e-a7c1f058a3d6")),
            new OrganizationId(Guid.Parse("ad20d74a-9d82-4eba-a81e-a7c1f058a3d6")), Start, Start.AddMinutes(15), out var session));
        return Assert.IsType<EmbeddedSignupSession>(session);
    }

    [Fact]
    public void New_session_is_pending_without_completion_time()
    {
        var session = Session();
        Assert.Equal(EmbeddedSignupSessionStatus.Pending, session.Status);
        Assert.Null(session.CompletedAt);
    }

    [Fact]
    public void Pending_session_completes_with_a_utc_timestamp()
    {
        var session = Session();
        Assert.Equal(SignupTransition.Applied, session.Complete(Start.AddMinutes(1)));
        Assert.Equal(EmbeddedSignupSessionStatus.Completed, session.Status);
        Assert.Equal(Start.AddMinutes(1), session.CompletedAt);
    }

    [Fact]
    public void Completed_session_cannot_complete_again_or_rewrite_completion_time()
    {
        var session = Session();
        session.Complete(Start.AddMinutes(1));
        Assert.Equal(SignupTransition.NotPending, session.Complete(Start.AddMinutes(2)));
        Assert.Equal(Start.AddMinutes(1), session.CompletedAt);
    }

    [Fact]
    public void Failed_session_cannot_complete()
    {
        var session = Session();
        Assert.Equal(SignupTransition.Applied, session.Fail(Start.AddMinutes(1)));
        Assert.Equal(SignupTransition.NotPending, session.Complete(Start.AddMinutes(2)));
        Assert.Equal(EmbeddedSignupSessionStatus.Failed, session.Status);
        Assert.Null(session.CompletedAt);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(16)]
    public void Session_cannot_complete_at_or_after_expiration(int elapsedMinutes)
    {
        var session = Session();
        Assert.Equal(SignupTransition.Expired, session.Complete(Start.AddMinutes(elapsedMinutes)));
        Assert.Equal(EmbeddedSignupSessionStatus.Expired, session.Status);
        Assert.Null(session.CompletedAt);
        Assert.Equal(SignupTransition.NotPending, session.Complete(Start.AddMinutes(17)));
    }

    [Fact]
    public void Invalid_completion_time_does_not_change_session_state()
    {
        var session = Session();
        Assert.Equal(SignupTransition.InvalidTimestamp, session.Complete(Start.AddSeconds(-1)));
        Assert.Equal(SignupTransition.InvalidTimestamp, session.Complete(Start.ToOffset(TimeSpan.FromHours(1))));
        Assert.Equal(EmbeddedSignupSessionStatus.Pending, session.Status);
        Assert.Null(session.CompletedAt);
    }
}

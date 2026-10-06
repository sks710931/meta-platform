namespace WhatsAppPlatform.Domain.WhatsAppAccounts;

public enum SignupTransition
{
    Applied = 1,
    NotPending = 2,
    Expired = 3,
    InvalidTimestamp = 4
}

namespace WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

public enum OnboardingError { Invalid, NotFound, Conflict, Expired, Disabled, RestartRequired, ProviderUnavailable, ProviderRejected }

public sealed class OnboardingResult<T>
{
    public T? Value { get; }
    public OnboardingError? Error { get; }
    public string? Message { get; }
    private OnboardingResult(T? value, OnboardingError? error, string? message)
    { Value = value; Error = error; Message = message; }
    public static OnboardingResult<T> Success(T value) => new(value, null, null);
    public static OnboardingResult<T> Failure(OnboardingError error, string message) => new(default, error, message);
}

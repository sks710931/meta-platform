using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

internal static class OnboardingHttpResults
{
    public static bool TryIdentifier(string raw, out Guid value) => Guid.TryParse(raw, out value) && value != Guid.Empty;
    public static IResult InvalidIdentifier(string field) => Results.ValidationProblem(
        new Dictionary<string, string[]> { [field] = ["Identifier must be a nonempty UUID."] });
    public static IResult Error(OnboardingError? error, string? message) => error switch
    {
        OnboardingError.Invalid => Results.ValidationProblem(new Dictionary<string, string[]> { ["result"] = [message ?? "Invalid input."] }),
        OnboardingError.NotFound => Results.Problem(statusCode: 404, title: "Not found", detail: message),
        OnboardingError.Conflict => Results.Problem(statusCode: 409, title: "Onboarding conflict", detail: message),
        OnboardingError.Expired => Results.Problem(statusCode: 410, title: "Session expired", detail: message),
        _ => throw new InvalidOperationException("Missing onboarding result.")
    };
}

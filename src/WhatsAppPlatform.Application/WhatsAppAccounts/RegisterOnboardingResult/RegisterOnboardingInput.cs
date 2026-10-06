namespace WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;

// Temporary simulation input, NOT a Meta Graph/Embedded Signup wire DTO. No credential fields.
public sealed record RegisterPhoneInput(string? ExternalPhoneNumberId, string? DisplayPhoneNumber, string? VerifiedName);
public sealed record RegisterOnboardingInput(string? ExternalWhatsAppAccountId, string? ExternalMessagingAccountId,
    string? DisplayName, IReadOnlyList<RegisterPhoneInput?>? PhoneNumbers);

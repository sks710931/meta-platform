namespace WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;

// Application registration contract, NOT a Meta transport DTO. No credentials; null WAAC identity is deferred.
public sealed record RegisterPhoneInput(string? ExternalPhoneNumberId, string? DisplayPhoneNumber, string? VerifiedName);
public sealed record RegisterOnboardingInput(string? ExternalWhatsAppAccountId, string? ExternalMessagingAccountId,
    string? DisplayName, IReadOnlyList<RegisterPhoneInput?>? PhoneNumbers);

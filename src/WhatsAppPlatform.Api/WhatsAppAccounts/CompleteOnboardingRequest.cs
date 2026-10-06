using System.Text.Json.Serialization;
using WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;

namespace WhatsAppPlatform.Api.WhatsAppAccounts;

// Development-only simulation contract. Reject unknown fields rather than accepting credentials.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompletePhoneRequest(string? ExternalPhoneNumberId, string? DisplayPhoneNumber, string? VerifiedName);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompleteOnboardingRequest(string? ExternalWhatsAppAccountId, string? ExternalMessagingAccountId,
    string? DisplayName, IReadOnlyList<CompletePhoneRequest?>? PhoneNumbers)
{
    public RegisterOnboardingInput ToInput() => new(ExternalWhatsAppAccountId, ExternalMessagingAccountId, DisplayName,
        PhoneNumbers?.Select(phone => phone is null ? null : new RegisterPhoneInput(
            phone.ExternalPhoneNumberId, phone.DisplayPhoneNumber, phone.VerifiedName)).ToArray());
}

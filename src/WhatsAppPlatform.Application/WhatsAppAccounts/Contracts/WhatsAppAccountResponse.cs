namespace WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

public sealed record PhoneNumberResponse(Guid PhoneNumberId, string ExternalPhoneNumberId,
    string DisplayPhoneNumber, string? VerifiedName, string Status, DateTimeOffset CreatedAt);
public sealed record MessagingAccountResponse(Guid MessagingAccountId, string ExternalMessagingAccountId, DateTimeOffset CreatedAt);
public sealed record WhatsAppAccountResponse(Guid WhatsAppAccountId, Guid OrganizationId, string ExternalWhatsAppAccountId,
    string DisplayName, string Status, DateTimeOffset CreatedAt, DateTimeOffset? ConnectedAt,
    MessagingAccountResponse MessagingAccount, IReadOnlyList<PhoneNumberResponse> PhoneNumbers)
{
    public static WhatsAppAccountResponse FromRegistration(RegisteredAccount registration) => new(
        registration.Account.Id.Value, registration.Account.OrganizationId.Value,
        registration.Account.ExternalWhatsAppAccountId.Value, registration.Account.DisplayName,
        registration.Account.Status.ToString(), registration.Account.CreatedAt, registration.Account.ConnectedAt,
        new(registration.MessagingAccount.Id.Value, registration.MessagingAccount.ExternalMessagingAccountId.Value,
            registration.MessagingAccount.CreatedAt),
        registration.PhoneNumbers.OrderBy(phone => phone.ExternalPhoneNumberId.Value, StringComparer.Ordinal)
            .Select(phone => new PhoneNumberResponse(phone.Id.Value, phone.ExternalPhoneNumberId.Value,
                phone.DisplayPhoneNumber, phone.VerifiedName, phone.Status.ToString(), phone.CreatedAt)).ToArray());
}

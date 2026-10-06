using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.RegisterOnboardingResult;

internal static class RegistrationDraft
{
    public static RegisteredAccount? Create(EmbeddedSignupSession session, RegisterOnboardingInput input, DateTimeOffset now)
    {
        ExternalWhatsAppAccountId? externalAccountId = null;
        if (input.ExternalWhatsAppAccountId is not null &&
            !ExternalWhatsAppAccountId.TryCreate(input.ExternalWhatsAppAccountId, out externalAccountId)) return null;
        if (!ExternalMessagingAccountId.TryCreate(input.ExternalMessagingAccountId, out var externalMessagingId) || externalMessagingId is null ||
            input.PhoneNumbers is null || input.PhoneNumbers.Count is < 1 or > 100) return null;
        var accountId = new WhatsAppAccountId(Guid.NewGuid());
        if (!WhatsAppAccount.TryCreateConnected(accountId, session.OrganizationId, session.Id,
            externalAccountId, input.DisplayName, now, out var account) || account is null ||
            !MessagingAccount.TryCreate(new MessagingAccountId(Guid.NewGuid()), accountId, externalMessagingId,
                now, out var messaging) || messaging is null) return null;
        var phones = new List<PhoneNumber>();
        var externalIds = new HashSet<ExternalPhoneNumberId>();
        foreach (var inputPhone in input.PhoneNumbers)
        {
            if (inputPhone is null || !ExternalPhoneNumberId.TryCreate(inputPhone.ExternalPhoneNumberId, out var externalId) ||
                externalId is null || !externalIds.Add(externalId) ||
                !PhoneNumber.TryCreate(new PhoneNumberId(Guid.NewGuid()), accountId, externalId,
                    inputPhone.DisplayPhoneNumber, inputPhone.VerifiedName, now, out var phone) || phone is null) return null;
            phones.Add(phone);
        }
        return new(account, messaging, phones);
    }

    public static bool Matches(RegisteredAccount existing, RegisteredAccount incoming) =>
        existing.Account.ExternalWhatsAppAccountId == incoming.Account.ExternalWhatsAppAccountId &&
        existing.Account.DisplayName == incoming.Account.DisplayName &&
        existing.MessagingAccount.ExternalMessagingAccountId == incoming.MessagingAccount.ExternalMessagingAccountId &&
        existing.PhoneNumbers.OrderBy(phone => phone.ExternalPhoneNumberId.Value, StringComparer.Ordinal)
            .Select(phone => (phone.ExternalPhoneNumberId, phone.DisplayPhoneNumber, phone.VerifiedName))
            .SequenceEqual(incoming.PhoneNumbers.OrderBy(phone => phone.ExternalPhoneNumberId.Value, StringComparer.Ordinal)
                .Select(phone => (phone.ExternalPhoneNumberId, phone.DisplayPhoneNumber, phone.VerifiedName)));
}

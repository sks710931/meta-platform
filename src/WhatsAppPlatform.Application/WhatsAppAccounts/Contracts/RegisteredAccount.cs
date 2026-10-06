using WhatsAppPlatform.Domain.WhatsAppAccounts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;

public sealed record RegisteredAccount(WhatsAppAccount Account, MessagingAccount MessagingAccount, IReadOnlyList<PhoneNumber> PhoneNumbers);

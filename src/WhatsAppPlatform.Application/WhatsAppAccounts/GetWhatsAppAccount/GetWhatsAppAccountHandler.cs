using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.GetWhatsAppAccount;

public sealed class GetWhatsAppAccountHandler(IWhatsAppAccountStore store)
{
    public async Task<OnboardingResult<WhatsAppAccountResponse>> HandleAsync(
        WhatsAppAccountId accountId, CancellationToken cancellationToken)
    {
        var account = await store.FindAccountAsync(accountId, cancellationToken);
        return account is null ? OnboardingResult<WhatsAppAccountResponse>.Failure(OnboardingError.NotFound, "Account not found.")
            : OnboardingResult<WhatsAppAccountResponse>.Success(WhatsAppAccountResponse.FromRegistration(account));
    }
}

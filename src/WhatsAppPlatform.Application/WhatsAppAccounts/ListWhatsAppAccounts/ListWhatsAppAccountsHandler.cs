using WhatsAppPlatform.Application.Organizations.Contracts;
using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Application.WhatsAppAccounts.ListWhatsAppAccounts;

public sealed class ListWhatsAppAccountsHandler(IOrganizationStore organizations, IWhatsAppAccountStore store)
{
    public async Task<OnboardingResult<IReadOnlyList<WhatsAppAccountResponse>>> HandleAsync(
        OrganizationId organizationId, CancellationToken cancellationToken)
    {
        if (await organizations.FindAsync(organizationId, cancellationToken) is null)
            return OnboardingResult<IReadOnlyList<WhatsAppAccountResponse>>.Failure(OnboardingError.NotFound, "Organization not found.");
        var accounts = await store.ListAccountsAsync(organizationId, cancellationToken);
        return OnboardingResult<IReadOnlyList<WhatsAppAccountResponse>>.Success(accounts.Select(WhatsAppAccountResponse.FromRegistration).ToArray());
    }
}

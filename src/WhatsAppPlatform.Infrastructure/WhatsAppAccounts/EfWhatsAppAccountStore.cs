using Microsoft.EntityFrameworkCore;
using Npgsql;
using WhatsAppPlatform.Application.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Domain.Organizations.Contracts;
using WhatsAppPlatform.Domain.WhatsAppAccounts;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.WhatsAppAccounts;

internal sealed class EfWhatsAppAccountStore(PlatformDbContext context) : IWhatsAppAccountStore
{
    public async Task AddSessionAsync(EmbeddedSignupSession session, CancellationToken cancellationToken)
    {
        context.Set<EmbeddedSignupSession>().Add(session);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<EmbeddedSignupSession?> FindSessionAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken) =>
        context.Set<EmbeddedSignupSession>().SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);

    public Task<SaveOutcome> SaveSessionAsync(CancellationToken cancellationToken) => SaveAsync(cancellationToken);

    public Task<SaveOutcome> SaveRegistrationAsync(RegisteredAccount registration, CancellationToken cancellationToken)
    {
        context.Set<WhatsAppAccount>().Add(registration.Account);
        context.Set<MessagingAccount>().Add(registration.MessagingAccount);
        context.Set<PhoneNumber>().AddRange(registration.PhoneNumbers);
        // SaveChanges wraps all inserts plus the tracked session update in one transaction.
        return SaveAsync(cancellationToken);
    }

    private async Task<SaveOutcome> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return SaveOutcome.Saved;
        }
        catch (DbUpdateConcurrencyException)
        {
            context.ChangeTracker.Clear();
            return SaveOutcome.Conflict;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.ChangeTracker.Clear();
            return SaveOutcome.Conflict;
        }
    }

    public async Task<RegisteredAccount?> FindRegistrationAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken)
    {
        var account = await context.Set<WhatsAppAccount>().AsNoTracking()
            .SingleOrDefaultAsync(account => account.SignupSessionId == sessionId, cancellationToken);
        return account is null ? null : (await ReadGraphsAsync([account], cancellationToken)).Single();
    }

    public async Task<RegisteredAccount?> FindAccountAsync(WhatsAppAccountId accountId, CancellationToken cancellationToken)
    {
        var account = await context.Set<WhatsAppAccount>().AsNoTracking()
            .SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken);
        return account is null ? null : (await ReadGraphsAsync([account], cancellationToken)).Single();
    }

    public async Task<IReadOnlyList<RegisteredAccount>> ListAccountsAsync(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var accounts = await context.Set<WhatsAppAccount>().AsNoTracking()
            .Where(account => account.OrganizationId == organizationId)
            .OrderByDescending(account => account.CreatedAt).ThenByDescending(account => account.Id).ToListAsync(cancellationToken);
        return await ReadGraphsAsync(accounts, cancellationToken);
    }

    private async Task<IReadOnlyList<RegisteredAccount>> ReadGraphsAsync(
        IReadOnlyList<WhatsAppAccount> accounts, CancellationToken cancellationToken)
    {
        if (accounts.Count == 0) return [];
        var accountIds = accounts.Select(account => account.Id).ToArray();
        var messaging = await context.Set<MessagingAccount>().AsNoTracking()
            .Where(account => accountIds.Contains(account.WhatsAppAccountId)).ToListAsync(cancellationToken);
        var phones = await context.Set<PhoneNumber>().AsNoTracking()
            .Where(phone => accountIds.Contains(phone.WhatsAppAccountId)).ToListAsync(cancellationToken);
        return accounts.Select(account => new RegisteredAccount(account,
            messaging.Single(item => item.WhatsAppAccountId == account.Id),
            phones.Where(phone => phone.WhatsAppAccountId == account.Id).ToArray())).ToArray();
    }
}

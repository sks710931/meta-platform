using System.Security.Cryptography;
using System.Data.Common;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Meta.Credentials;

internal sealed class ProtectedMetaCredentialStore(PlatformDbContext context, IDataProtectionProvider protection,
    TimeProvider clock) : IMetaCredentialStore
{
    private readonly IDataProtector protector = protection.CreateProtector("WhatsAppPlatform.Meta.CustomerCredential.v1");
    public async Task<StoredMetaCredential?> FindAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken)
    {
        var record = await context.Set<MetaCredentialRecord>().SingleOrDefaultAsync(record => record.SessionId == sessionId, cancellationToken);
        if (record is null) return null;
        if (record.ProtectedCredential is null) return new(null);
        try { return new(new MetaCredential(protector.CreateProtector(sessionId.Value.ToString("D")).Unprotect(record.ProtectedCredential))); }
        catch (CryptographicException) { return new(null); } // Missing/changed key ring requires a fresh signup.
    }

    public async Task<bool> ReserveExchangeAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        context.Set<MetaCredentialRecord>().Add(new() { Id = Guid.NewGuid(), SessionId = sessionId, CreatedAt = now, UpdatedAt = now });
        return await SaveCheckpointAsync(cancellationToken);
    }

    public async Task<bool> ProtectAndPersistAsync(EmbeddedSignupSessionId sessionId, MetaCredential credential, CancellationToken cancellationToken)
    {
        try
        {
            var record = await context.Set<MetaCredentialRecord>().SingleAsync(record => record.SessionId == sessionId, cancellationToken);
            record.ProtectedCredential = protector.CreateProtector(sessionId.Value.ToString("D")).Protect(credential.AccessToken);
            record.UpdatedAt = clock.GetUtcNow();
            return await SaveCheckpointAsync(cancellationToken);
        }
        catch (CryptographicException) { return false; }
        catch (DbException) { return false; }
    }

    private async Task<bool> SaveCheckpointAsync(CancellationToken cancellationToken)
    {
        try { await context.SaveChangesAsync(cancellationToken); return true; }
        catch (DbUpdateException) { context.ChangeTracker.Clear(); return false; }
    }
}

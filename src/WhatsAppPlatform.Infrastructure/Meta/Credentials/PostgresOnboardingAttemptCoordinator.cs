using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WhatsAppPlatform.Application.WhatsAppAccounts.MetaEmbeddedSignup;
using WhatsAppPlatform.Domain.WhatsAppAccounts.Contracts;
using WhatsAppPlatform.Infrastructure.Persistence;

namespace WhatsAppPlatform.Infrastructure.Meta.Credentials;

internal sealed class PostgresOnboardingAttemptCoordinator(PlatformDbContext context) : IOnboardingAttemptCoordinator
{
    public async Task<IAsyncDisposable?> TryAcquireAsync(EmbeddedSignupSessionId sessionId, CancellationToken cancellationToken)
    {
        var key = BinaryPrimitives.ReadInt64BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes($"meta-signup:{sessionId.Value:D}")));
        var connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(context.Database.GetConnectionString()) { Pooling = false }.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@key)", connection);
            command.Parameters.AddWithValue("key", key);
            if (await command.ExecuteScalarAsync(cancellationToken) is true) return new Lease(connection);
            await connection.DisposeAsync();
            return null;
        }
        catch { await connection.DisposeAsync(); throw; }
    }

    // Dedicated unpooled connection: physical close releases the session lock even on cancellation.
    private sealed class Lease(NpgsqlConnection connection) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => connection.DisposeAsync();
    }
}

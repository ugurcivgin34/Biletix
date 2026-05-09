using Biletix.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Biletix.Infrastructure.Redis;

/// <summary>
/// Idempotency response cache'ini Redis uzerinde saklar.
/// </summary>
public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IDatabase _db;

    /// <summary>
    /// Redis baglantisindan database nesnesini alir.
    /// </summary>
    /// <param name="connectionMultiplexer">Redis baglanti yoneticisi.</param>
    public IdempotencyService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }

    /// <inheritdoc />
    public async Task<string?> GetCachedResponseAsync(string idempotencyKey)
    {
        var value = await _db.StringGetAsync(BuildKey(idempotencyKey));
        return value.HasValue ? value.ToString() : null;
    }

    /// <inheritdoc />
    public Task CacheResponseAsync(string idempotencyKey, string response, TimeSpan expiry)
    {
        return _db.StringSetAsync(BuildKey(idempotencyKey), response, expiry);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string idempotencyKey)
    {
        return _db.KeyExistsAsync(BuildKey(idempotencyKey));
    }

    private static string BuildKey(string idempotencyKey)
    {
        return $"idempotency:{idempotencyKey}";
    }
}

using Biletix.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Biletix.Infrastructure.Redis;

/// <summary>
/// Bilet tipi rezervasyon kilitlerini Redis uzerinde atomik komutlarla yonetir.
/// </summary>
public sealed class TicketLockService : ITicketLockService
{
    private readonly IDatabase _db;

    /// <summary>
    /// Redis baglantisindan database nesnesini alir.
    /// </summary>
    /// <param name="connectionMultiplexer">Redis baglanti yoneticisi.</param>
    public TicketLockService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }

    /// <inheritdoc />
    public Task<bool> AcquireLockAsync(Guid ticketTypeId, Guid userId, TimeSpan expiry)
    {
        return _db.StringSetAsync(
            BuildKey(ticketTypeId),
            userId.ToString(),
            expiry,
            When.NotExists);
    }

    /// <inheritdoc />
    public async Task<bool> ReleaseLockAsync(Guid ticketTypeId, Guid userId)
    {
        const string luaScript = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        var result = await _db.ScriptEvaluateAsync(
            luaScript,
            new RedisKey[] { BuildKey(ticketTypeId) },
            new RedisValue[] { userId.ToString() });

        return (long)result == 1;
    }

    /// <inheritdoc />
    public async Task<Guid?> GetLockOwnerAsync(Guid ticketTypeId)
    {
        var value = await _db.StringGetAsync(BuildKey(ticketTypeId));

        if (!value.HasValue)
        {
            return null;
        }

        return Guid.TryParse(value.ToString(), out var userId) ? userId : null;
    }

    /// <inheritdoc />
    public async Task<bool> ExtendLockAsync(Guid ticketTypeId, Guid userId, TimeSpan extension)
    {
        const string luaScript = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('expire', KEYS[1], ARGV[2])
            else
                return 0
            end";

        var result = await _db.ScriptEvaluateAsync(
            luaScript,
            new RedisKey[] { BuildKey(ticketTypeId) },
            new RedisValue[] { userId.ToString(), (long)extension.TotalSeconds });

        return (long)result == 1;
    }

    private static string BuildKey(Guid ticketTypeId)
    {
        return $"ticket_lock:{ticketTypeId}";
    }
}

using Biletix.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Biletix.Infrastructure.Redis;

/// <summary>
/// Etkinlik bazli sanal bekleme sirasini Redis SortedSet uzerinde yonetir.
/// </summary>
public sealed class WaitingQueueService : IWaitingQueueService
{
    private const int ActiveSlots = 500;
    private const int SecondsPerSlot = 30;

    private readonly IDatabase _db;

    /// <summary>
    /// Redis baglantisindan database nesnesini alir.
    /// </summary>
    /// <param name="connectionMultiplexer">Redis baglanti yoneticisi.</param>
    public WaitingQueueService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }

    /// <inheritdoc />
    public async Task<long> EnqueueAsync(Guid eventId, Guid userId)
    {
        var key = BuildKey(eventId);
        var score = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await _db.SortedSetAddAsync(key, userId.ToString(), score, When.NotExists); // Kullanici zaten varsa ekleme, yeni eklenen kullanicilar sona gelir.

        var position = await _db.SortedSetRankAsync(key, userId.ToString()); // Kullanici siradaysa pozisyonunu getir, yoksa null doner.
        return (position ?? 0) + 1;
    }

    /// <inheritdoc />
    public async Task<long?> GetPositionAsync(Guid eventId, Guid userId)
    {
        var rank = await _db.SortedSetRankAsync(BuildKey(eventId), userId.ToString());
        return rank.HasValue ? rank.Value + 1 : null;
    }

    /// <inheritdoc />
    public Task<long> GetQueueLengthAsync(Guid eventId)
    {
        return _db.SortedSetLengthAsync(BuildKey(eventId));
    }

    /// <inheritdoc />
    public Task DequeueAsync(Guid eventId, Guid userId)
    {
        return _db.SortedSetRemoveAsync(BuildKey(eventId), userId.ToString());
    }

    /// <inheritdoc />
    public async Task<bool> CanProceedAsync(Guid eventId, Guid userId)
    {
        var position = await GetPositionAsync(eventId, userId);
        return position.HasValue && position.Value <= ActiveSlots;
    }

    /// <inheritdoc />
    public async Task<int> GetEstimatedWaitTimeAsync(Guid eventId, Guid userId)
    {
        var position = await GetPositionAsync(eventId, userId);
        if (!position.HasValue || position.Value <= ActiveSlots)
        {
            return 0;
        }

        var waitingBatches = (int)Math.Ceiling((position.Value - ActiveSlots) / (double)ActiveSlots);
        return waitingBatches * SecondsPerSlot;
    }

    // Etkinlik bazli siralar icin benzersiz Redis anahtari olusturur.
    private static string BuildKey(Guid eventId)
    {
        return $"queue:{eventId}";
    }
}

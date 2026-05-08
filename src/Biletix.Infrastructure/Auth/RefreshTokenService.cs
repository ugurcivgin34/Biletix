using Biletix.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Biletix.Infrastructure.Auth;

/// <summary>
/// Refresh token'lari Redis uzerinde sureli key'ler olarak saklar ve iptal eder.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IConnectionMultiplexer _redis;

    /// <summary>
    /// Redis baglantisini alir.
    /// </summary>
    /// <param name="redis">Redis connection multiplexer.</param>
    public RefreshTokenService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    /// <summary>
    /// Refresh token'i kullaniciya bagli key ile Redis'e yazar.
    /// </summary>
    /// <param name="userId">Token sahibi kullanici kimligi.</param>
    /// <param name="refreshToken">Saklanacak refresh token.</param>
    /// <param name="expiry">Token gecerlilik suresi.</param>
    public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, TimeSpan expiry)
    {
        var database = _redis.GetDatabase();
        await database.StringSetAsync(BuildKey(userId, refreshToken), "1", expiry);
    }

    /// <summary>
    /// Refresh token key'inin Redis'te bulunup bulunmadigini kontrol eder.
    /// </summary>
    /// <param name="userId">Token sahibi kullanici kimligi.</param>
    /// <param name="refreshToken">Dogrulanacak refresh token.</param>
    /// <returns>Token Redis'te varsa true.</returns>
    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var database = _redis.GetDatabase();
        return await database.KeyExistsAsync(BuildKey(userId, refreshToken));
    }

    /// <summary>
    /// Belirli refresh token key'ini Redis'ten siler.
    /// </summary>
    /// <param name="userId">Token sahibi kullanici kimligi.</param>
    /// <param name="refreshToken">Iptal edilecek refresh token.</param>
    public async Task RevokeRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var database = _redis.GetDatabase();
        await database.KeyDeleteAsync(BuildKey(userId, refreshToken));
    }

    /// <summary>
    /// Kullaniciya ait tum refresh token key'lerini Redis'ten siler.
    /// </summary>
    /// <param name="userId">Token'lari silinecek kullanici kimligi.</param>
    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var database = _redis.GetDatabase();
        var pattern = $"refresh_token:{userId}:*";
        var endpoints = _redis.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);

            foreach (var key in server.Keys(pattern: pattern))
            {
                await database.KeyDeleteAsync(key);
            }
        }
    }

    private static string BuildKey(Guid userId, string refreshToken)
    {
        return $"refresh_token:{userId}:{refreshToken}";
    }
}

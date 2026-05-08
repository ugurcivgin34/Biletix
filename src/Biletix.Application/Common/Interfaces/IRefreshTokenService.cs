namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Refresh token saklama, dogrulama ve iptal islemlerini soyutlayan servis sozlesmesidir.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Refresh token'i kullaniciye bagli ve sureli olacak sekilde saklar.
    /// </summary>
    /// <param name="userId">Token sahibi kullanici kimligi.</param>
    /// <param name="refreshToken">Saklanacak refresh token.</param>
    /// <param name="expiry">Token gecerlilik suresi.</param>
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken, TimeSpan expiry);

    /// <summary>
    /// Refresh token'in kullanici icin halen gecerli olup olmadigini kontrol eder.
    /// </summary>
    /// <param name="userId">Token sahibi kullanici kimligi.</param>
    /// <param name="refreshToken">Dogrulanacak refresh token.</param>
    /// <returns>Token gecerliyse true.</returns>
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);

    /// <summary>
    /// Belirli bir refresh token'i iptal eder.
    /// </summary>
    /// <param name="userId">Token sahibi kullanici kimligi.</param>
    /// <param name="refreshToken">Iptal edilecek refresh token.</param>
    Task RevokeRefreshTokenAsync(Guid userId, string refreshToken);

    /// <summary>
    /// Kullaniciya ait tum refresh token'lari iptal eder.
    /// </summary>
    /// <param name="userId">Token'lari iptal edilecek kullanici kimligi.</param>
    Task RevokeAllUserTokensAsync(Guid userId);
}

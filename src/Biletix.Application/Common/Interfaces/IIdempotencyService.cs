namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Idempotency-Key bazli cevap cache'leme operasyonlarini soyutlar.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Idempotency key icin onceden cache'lenmis response JSON degerini getirir.
    /// </summary>
    /// <param name="idempotencyKey">Idempotency anahtari.</param>
    /// <returns>Cache'lenmis response JSON degeri; yoksa null.</returns>
    Task<string?> GetCachedResponseAsync(string idempotencyKey);

    /// <summary>
    /// Response JSON degerini belirtilen sureyle cache'e yazar.
    /// </summary>
    /// <param name="idempotencyKey">Idempotency anahtari.</param>
    /// <param name="response">Cache'lenecek response JSON degeri.</param>
    /// <param name="expiry">Cache gecerlilik suresi.</param>
    Task CacheResponseAsync(string idempotencyKey, string response, TimeSpan expiry);

    /// <summary>
    /// Idempotency key icin cache kaydi olup olmadigini kontrol eder.
    /// </summary>
    /// <param name="idempotencyKey">Kontrol edilecek idempotency anahtari.</param>
    /// <returns>Kayit varsa true.</returns>
    Task<bool> ExistsAsync(string idempotencyKey);
}

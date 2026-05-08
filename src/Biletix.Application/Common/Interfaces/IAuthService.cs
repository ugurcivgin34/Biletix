namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Sifre hashleme ve dogrulama islemlerini soyutlayan servis sozlesmesidir.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Duz metin sifreyi guvenli hash degerine donusturur.
    /// </summary>
    /// <param name="password">Hashlenecek duz metin sifre.</param>
    /// <returns>Hashlenmis sifre.</returns>
    Task<string> HashPasswordAsync(string password);

    /// <summary>
    /// Duz metin sifrenin hash ile eslesip eslesmedigini kontrol eder.
    /// </summary>
    /// <param name="password">Dogrulanacak duz metin sifre.</param>
    /// <param name="hash">Kayitli sifre hash'i.</param>
    /// <returns>Sifre dogruysa true.</returns>
    Task<bool> VerifyPasswordAsync(string password, string hash);
}

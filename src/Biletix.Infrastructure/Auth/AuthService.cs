using Biletix.Application.Common.Interfaces;

namespace Biletix.Infrastructure.Auth;

/// <summary>
/// BCrypt kullanarak sifre hashleme ve dogrulama islemlerini gerceklestirir.
/// </summary>
public class AuthService : IAuthService
{
    /// <summary>
    /// Duz metin sifreyi BCrypt work factor 12 ile hashler.
    /// </summary>
    /// <param name="password">Hashlenecek duz metin sifre.</param>
    /// <returns>Hashlenmis sifre.</returns>
    public Task<string> HashPasswordAsync(string password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12));
    }

    /// <summary>
    /// Duz metin sifrenin kayitli BCrypt hash'i ile eslesip eslesmedigini kontrol eder.
    /// </summary>
    /// <param name="password">Dogrulanacak duz metin sifre.</param>
    /// <param name="hash">Kayitli BCrypt hash degeri.</param>
    /// <returns>Sifre dogruysa true.</returns>
    public Task<bool> VerifyPasswordAsync(string password, string hash)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, hash));
    }
}

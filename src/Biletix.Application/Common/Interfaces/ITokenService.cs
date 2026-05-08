using System.Security.Claims;
using Biletix.Domain.Entities;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// JWT access token ve refresh token uretimi ile token principal okuma islemlerini soyutlar.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Kullanici bilgilerini iceren imzali JWT access token uretir.
    /// </summary>
    /// <param name="user">Token uretilecek kullanici.</param>
    /// <returns>JWT access token.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Guvenli rastgele refresh token uretir.
    /// </summary>
    /// <returns>Refresh token degeri.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Suresi dolmus access token icinden principal bilgisini lifetime kontrolu yapmadan okur.
    /// </summary>
    /// <param name="token">Okunacak JWT token.</param>
    /// <returns>Token gecerliyse principal, aksi halde null.</returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

namespace Biletix.Application.Features.Auth.Commands.Login;

/// <summary>
/// Basarili giris veya token yenileme sonrasi dondurulen token cevap modelidir.
/// </summary>
/// <param name="AccessToken">JWT access token.</param>
/// <param name="RefreshToken">Refresh token.</param>
/// <param name="AccessTokenExpiry">Access token bitis zamani.</param>
/// <param name="UserId">Kullanici kimligi.</param>
/// <param name="Email">Kullanici e-posta adresi.</param>
/// <param name="FullName">Kullanicinin ad soyad bilgisi.</param>
/// <param name="Role">Kullanici rolu.</param>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    Guid UserId,
    string Email,
    string FullName,
    string Role);

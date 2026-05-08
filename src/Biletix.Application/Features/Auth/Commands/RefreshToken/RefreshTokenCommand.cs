using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Auth.Commands.Login;

namespace Biletix.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Suresi dolmus access token ve gecerli refresh token ile yeni token cifti almak icin kullanilan komuttur.
/// </summary>
public class RefreshTokenCommand : ICommand<LoginResponse>
{
    /// <summary>
    /// Suresi dolmus veya dolmak uzere olan access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Mevcut refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

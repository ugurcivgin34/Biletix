using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Belirli bir refresh token'i iptal ederek kullaniciyi cikis yaptirmak icin kullanilan komuttur.
/// </summary>
public class LogoutCommand : ICommand
{
    /// <summary>
    /// Cikis yapan kullanicinin kimligi.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Iptal edilecek refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

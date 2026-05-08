using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Auth.Commands.Login;

/// <summary>
/// Kullanici girisi yapmak icin kullanilan komuttur.
/// </summary>
public class LoginCommand : ICommand<LoginResponse>
{
    /// <summary>
    /// Giris yapacak kullanicinin e-posta adresi.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Giris yapacak kullanicinin duz metin sifresi.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

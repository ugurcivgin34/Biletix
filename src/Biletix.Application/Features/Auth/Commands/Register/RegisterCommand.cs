using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Auth.Commands.Register;

/// <summary>
/// Yeni kullanici kaydi olusturmak icin kullanilan komuttur.
/// </summary>
public class RegisterCommand : ICommand<RegisterResponse>
{
    /// <summary>
    /// Kayit olacak kullanicinin e-posta adresi.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Kayit olacak kullanicinin duz metin sifresi.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Kayit olacak kullanicinin adi.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Kayit olacak kullanicinin soyadi.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}

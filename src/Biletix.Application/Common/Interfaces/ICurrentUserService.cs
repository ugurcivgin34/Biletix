namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Aktif HTTP istegindeki kullanici kimlik ve rol bilgilerine erisim saglayan servis sozlesmesidir.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Authenticated kullanicinin kimligi; kullanici yoksa null.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Authenticated kullanicinin e-posta adresi; kullanici yoksa null.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Authenticated kullanicinin rol bilgisi; kullanici yoksa null.
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Mevcut istekte authenticated kullanici olup olmadigini belirtir.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Mevcut kullanicinin belirtilen role sahip olup olmadigini kontrol eder.
    /// </summary>
    /// <param name="role">Kontrol edilecek rol adi.</param>
    /// <returns>Kullanici belirtilen roldeyse true.</returns>
    bool IsInRole(string role);
}

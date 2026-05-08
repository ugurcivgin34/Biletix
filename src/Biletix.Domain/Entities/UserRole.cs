namespace Biletix.Domain.Entities;

/// <summary>
/// Kullanici hesabinin sistemdeki yetki rolunu belirtir.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Bilet satin alan standart kullanici roludur.
    /// </summary>
    Customer,

    /// <summary>
    /// Etkinlik duzenleyen kullanici roludur.
    /// </summary>
    Organizer,

    /// <summary>
    /// Sistem yonetimi yetkilerine sahip kullanici roludur.
    /// </summary>
    Admin
}

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Kaynak sahipligine dayali yetkilendirme kontrollerini soyutlayan servis sozlesmesidir.
/// </summary>
public interface IResourceAuthorizationService
{
    /// <summary>
    /// Mevcut kullanicinin belirtilen organizer'a ait etkinligi yonetip yonetemeyecegini kontrol eder.
    /// </summary>
    /// <param name="eventOrganizerId">Etkinligin organizer kullanici kimligi.</param>
    /// <returns>Kullanici etkinligi yonetebiliyorsa true.</returns>
    bool CanManageEvent(Guid eventOrganizerId);

    /// <summary>
    /// Mevcut kullanicinin belirtilen kullaniciya ait rezervasyonu yonetip yonetemeyecegini kontrol eder.
    /// </summary>
    /// <param name="bookingUserId">Rezervasyon sahibi kullanici kimligi.</param>
    /// <returns>Kullanici rezervasyonu yonetebiliyorsa true.</returns>
    bool CanManageBooking(Guid bookingUserId);
}

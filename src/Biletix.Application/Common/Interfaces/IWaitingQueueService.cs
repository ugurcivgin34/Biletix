namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Etkinlik bazli sanal bekleme sirasi operasyonlarini yoneten servis sozlesmesidir.
/// </summary>
public interface IWaitingQueueService
{
    /// <summary>
    /// Kullaniciyi etkinlik sirasina ekler ve 1 tabanli pozisyonunu dondurur.
    /// </summary>
    /// <param name="eventId">Sira olusturulan etkinlik kimligi.</param>
    /// <param name="userId">Siraya giren kullanici kimligi.</param>
    /// <returns>Kullanicinin 1 tabanli sira pozisyonu.</returns>
    Task<long> EnqueueAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Kullanicinin etkinlik sirasindaki mevcut 1 tabanli pozisyonunu getirir.
    /// </summary>
    /// <param name="eventId">Sira olusturulan etkinlik kimligi.</param>
    /// <param name="userId">Pozisyonu sorgulanan kullanici kimligi.</param>
    /// <returns>Kullanici siradaysa pozisyonu, degilse null.</returns>
    Task<long?> GetPositionAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Etkinlik sirasindaki toplam kullanici sayisini getirir.
    /// </summary>
    /// <param name="eventId">Sira olusturulan etkinlik kimligi.</param>
    /// <returns>Siradaki toplam kullanici sayisi.</returns>
    Task<long> GetQueueLengthAsync(Guid eventId);

    /// <summary>
    /// Kullaniciyi etkinlik sirasindan cikarir.
    /// </summary>
    /// <param name="eventId">Sira olusturulan etkinlik kimligi.</param>
    /// <param name="userId">Siradan cikarilacak kullanici kimligi.</param>
    Task DequeueAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Kullanicinin aktif slotlar icinde olup rezervasyona devam edip edemeyecegini kontrol eder.
    /// </summary>
    /// <param name="eventId">Sira olusturulan etkinlik kimligi.</param>
    /// <param name="userId">Kontrol edilecek kullanici kimligi.</param>
    /// <returns>Kullanici devam edebiliyorsa true.</returns>
    Task<bool> CanProceedAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Kullanicinin tahmini bekleme suresini saniye cinsinden hesaplar.
    /// </summary>
    /// <param name="eventId">Sira olusturulan etkinlik kimligi.</param>
    /// <param name="userId">Bekleme suresi hesaplanacak kullanici kimligi.</param>
    /// <returns>Tahmini bekleme suresi, saniye.</returns>
    Task<int> GetEstimatedWaitTimeAsync(Guid eventId, Guid userId);
}

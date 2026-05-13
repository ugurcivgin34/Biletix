namespace Biletix.Application.Features.Queue.DTOs;

/// <summary>
/// Kullanicinin etkinlik bekleme sirasi durumunu temsil eder.
/// </summary>
/// <param name="EventId">Sira olusturulan etkinlik kimligi.</param>
/// <param name="UserId">Sirasi sorgulanan kullanici kimligi.</param>
/// <param name="Position">Kullanicinin 1 tabanli pozisyonu; sirada degilse 0.</param>
/// <param name="TotalInQueue">Etkinlik sirasindaki toplam kullanici sayisi.</param>
/// <param name="CanProceed">Kullanici rezervasyon akisina devam edebiliyorsa true.</param>
/// <param name="EstimatedWaitSeconds">Tahmini bekleme suresi, saniye.</param>
/// <param name="IsInQueue">Kullanici siradaysa true.</param>
public sealed record QueueStatusResponse(
    Guid EventId,
    Guid UserId,
    long Position,
    long TotalInQueue,
    bool CanProceed,
    int EstimatedWaitSeconds,
    bool IsInQueue);

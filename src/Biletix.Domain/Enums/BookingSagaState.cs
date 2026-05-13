namespace Biletix.Domain.Enums;

/// <summary>
/// Rezervasyon checkout saga'sinin ilerleme ve kompansasyon durumlarini belirtir.
/// </summary>
public enum BookingSagaState
{
    /// <summary>
    /// Saga basladi.
    /// </summary>
    Started,

    /// <summary>
    /// Biletler gecici olarak rezerve edildi.
    /// </summary>
    TicketsReserved,

    /// <summary>
    /// Odeme niyeti olusturuldu.
    /// </summary>
    PaymentIntentCreated,

    /// <summary>
    /// Odeme webhook ile dogrulandi.
    /// </summary>
    PaymentConfirmed,

    /// <summary>
    /// Basarisizlik sonrasi geri alma islemleri basladi.
    /// </summary>
    Compensating,

    /// <summary>
    /// Geri alma islemleri tamamlandi.
    /// </summary>
    Compensated,

    /// <summary>
    /// Saga basarisiz oldu ve manuel mudahale gerekebilir.
    /// </summary>
    Failed
}

namespace Biletix.Domain.Entities;

/// <summary>
/// Rezervasyonun odeme ve gecerlilik durumunu belirtir.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// Rezervasyon olusturulmus, odeme veya onay bekliyordur.
    /// </summary>
    Pending,

    /// <summary>
    /// Rezervasyon odemesi tamamlanmis ve kesinlesmistir.
    /// </summary>
    Confirmed,

    /// <summary>
    /// Rezervasyon iptal edilmistir.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Rezervasyon suresi dolmustur.
    /// </summary>
    Expired
}

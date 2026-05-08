namespace Biletix.Domain.Entities;

/// <summary>
/// Etkinligin yayin ve yasam dongusu durumunu belirtir.
/// </summary>
public enum EventStatus
{
    /// <summary>
    /// Etkinlik henuz hazirlik asamasindadir.
    /// </summary>
    Draft,

    /// <summary>
    /// Etkinlik kullanicilara acilmistir.
    /// </summary>
    Published,

    /// <summary>
    /// Etkinlik iptal edilmistir.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Etkinlik tamamlanmistir.
    /// </summary>
    Completed
}

using Biletix.Domain.Base;

namespace Biletix.Domain.Entities;

/// <summary>
/// Etkinlik girisinde okutulan QR bilet dogrulama denemesini temsil eder.
/// </summary>
public class TicketScan : BaseEntity<Guid>
{
    /// <summary>
    /// QR token'inin ait oldugu rezervasyon kimligidir.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// QR token'inin ait oldugu kullanici kimligidir.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// QR token'inin ait oldugu etkinlik kimligidir.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Biletin okutuldugu UTC zamandir.
    /// </summary>
    public DateTime ScannedAt { get; set; }

    /// <summary>
    /// Taramayi yapan kapi gorevlisi, cihaz veya turnike kimligidir.
    /// </summary>
    public string ScannedBy { get; set; } = string.Empty;

    /// <summary>
    /// Taramanin gecerli giris uretip uretmedigini belirtir.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gecersiz taramalarda neden bilgisidir.
    /// </summary>
    public string? InvalidReason { get; set; }
}

namespace Biletix.Application.Common.Models;

/// <summary>
/// Elasticsearch uzerinde etkinlik arama icin tutulan dokuman modelidir.
/// </summary>
public class EventSearchDocument
{
    /// <summary>
    /// Etkinligin benzersiz kimligi.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Etkinligin gorunen basligi.
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Etkinligin detay aciklamasi.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Etkinligin baslangic tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Etkinligin bitis tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Etkinligin yayin durumu.
    /// </summary>
    public string Status { get; set; } = default!;

    /// <summary>
    /// Etkinlik gorsel adresi.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Etkinligin mekan kimligi.
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Etkinligin mekan adi.
    /// </summary>
    public string VenueName { get; set; } = default!;

    /// <summary>
    /// Etkinligin mekan sehri.
    /// </summary>
    public string VenueCity { get; set; } = default!;

    /// <summary>
    /// Etkinligin mekan kapasitesi.
    /// </summary>
    public int VenueCapacity { get; set; }

    /// <summary>
    /// Etkinligin performer kimligi.
    /// </summary>
    public Guid PerformerId { get; set; }

    /// <summary>
    /// Etkinligin performer adi.
    /// </summary>
    public string PerformerName { get; set; } = default!;

    /// <summary>
    /// Etkinligin performer turu.
    /// </summary>
    public string PerformerGenre { get; set; } = default!;

    /// <summary>
    /// Etkinlikteki en dusuk bilet fiyati.
    /// </summary>
    public decimal MinPrice { get; set; }

    /// <summary>
    /// Etkinlikteki toplam uygun bilet sayisi.
    /// </summary>
    public int TotalAvailableTickets { get; set; }

    /// <summary>
    /// Etkinligin olusturulma zamani.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Etkinligin son guncellenme zamani.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

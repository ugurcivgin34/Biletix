using Biletix.Domain.Base;
using Biletix.Domain.Exceptions;

namespace Biletix.Domain.Entities;

/// <summary>
/// Bir etkinlik icindeki fiyat ve kapasite kurallarina sahip bilet kategorisini temsil eder.
/// </summary>
public class TicketType : BaseEntity<Guid>
{
    private TicketType()
    {
    }

    /// <summary>
    /// Bilet tipinin ait oldugu etkinlik kimligidir.
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// Bilet tipinin gorunen adidir.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Bilet tipinin birim fiyatidir.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Bu bilet tipi icin satilabilecek toplam kapasitedir.
    /// </summary>
    public int TotalCapacity { get; private set; }

    /// <summary>
    /// Kesin satisi tamamlanmis bilet sayisidir.
    /// </summary>
    public int SoldCount { get; private set; }

    /// <summary>
    /// Odeme veya onay bekleyen rezervasyonlardaki bilet sayisidir.
    /// </summary>
    public int ReservedCount { get; private set; }

    /// <summary>
    /// Satis veya rezervasyon icin halen kullanilabilir bilet sayisidir.
    /// </summary>
    public int AvailableCount => TotalCapacity - SoldCount - ReservedCount;

    /// <summary>
    /// Gecerli fiyat ve kapasiteyle yeni bilet tipi olusturur.
    /// </summary>
    /// <param name="eventId">Bilet tipinin ait oldugu etkinlik kimligi.</param>
    /// <param name="name">Bilet tipi adi.</param>
    /// <param name="price">Birim fiyat.</param>
    /// <param name="totalCapacity">Toplam kapasite.</param>
    /// <returns>Yeni bilet tipi.</returns>
    /// <exception cref="DomainException">Fiyat, kapasite veya ad gecersizse firlatilir.</exception>
    public static TicketType Create(Guid eventId, string name, decimal price, int totalCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Ticket type name cannot be empty");
        }

        if (price < 0)
        {
            throw new DomainException("Ticket type price cannot be negative");
        }

        if (totalCapacity <= 0)
        {
            throw new DomainException("Ticket type capacity must be greater than zero");
        }

        var utcNow = DateTime.UtcNow;

        return new TicketType
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Name = name,
            Price = price,
            TotalCapacity = totalCapacity,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    /// <summary>
    /// Belirtilen miktarda bileti gecici olarak rezerve eder.
    /// </summary>
    /// <param name="count">Rezerve edilecek bilet sayisi.</param>
    /// <exception cref="DomainException">Yeterli bilet yoksa veya miktar gecersizse firlatilir.</exception>
    public void Reserve(int count = 1)
    {
        if (count <= 0)
        {
            throw new DomainException("Reservation count must be greater than zero");
        }

        if (AvailableCount < count)
        {
            throw new DomainException($"Not enough available tickets. Available: {AvailableCount}");
        }

        ReservedCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bekleyen rezervasyondan belirtilen miktari serbest birakir.
    /// </summary>
    /// <param name="count">Serbest birakilacak bilet sayisi.</param>
    public void ReleaseReservation(int count = 1)
    {
        ReservedCount = Math.Max(0, ReservedCount - count);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rezerve edilmis biletleri kesin satisa cevirir.
    /// </summary>
    /// <param name="count">Kesin satisa cevrilecek bilet sayisi.</param>
    /// <exception cref="DomainException">Miktar gecersizse veya rezervasyon yetersizse firlatilir.</exception>
    public void ConfirmSale(int count = 1)
    {
        if (count <= 0)
        {
            throw new DomainException("Sale count must be greater than zero");
        }

        if (ReservedCount < count)
        {
            throw new DomainException("Cannot confirm sale: not enough reserved tickets");
        }

        ReservedCount -= count;
        SoldCount += count;
        UpdatedAt = DateTime.UtcNow;
    }
}

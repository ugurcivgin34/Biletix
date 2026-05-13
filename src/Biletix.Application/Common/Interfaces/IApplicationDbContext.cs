using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Application katmaninin veriye erisim icin ihtiyac duydugu DbContext sozlesmesidir.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Kullanici aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<User> Users { get; }

    /// <summary>
    /// Etkinlik aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<Event> Events { get; }

    /// <summary>
    /// Mekan aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<Venue> Venues { get; }

    /// <summary>
    /// Performer aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<Performer> Performers { get; }

    /// <summary>
    /// Rezervasyon aggregate'leri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<Booking> Bookings { get; }

    /// <summary>
    /// Rezervasyon kalemleri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<BookingItem> BookingItems { get; }

    /// <summary>
    /// Etkinliklere ait bilet tipleri icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<TicketType> TicketTypes { get; }

    /// <summary>
    /// Transactional outbox mesajlari icin sorgu ve kalicilik giris noktasi.
    /// </summary>
    DbSet<OutboxMessage> OutboxMessages { get; }

    /// <summary>
    /// Bekleyen degisiklikleri veritabanina kaydeder.
    /// </summary>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Veritabanina yazilan kayit sayisi.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

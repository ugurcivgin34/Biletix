using Biletix.Domain.Base;
using Biletix.Domain.Exceptions;

namespace Biletix.Domain.Entities;

/// <summary>
/// Etkinligin fiziksel olarak gerceklestigi mekan aggregate'ini temsil eder.
/// </summary>
public class Venue : AggregateRoot<Guid>
{
    private Venue()
    {
    }

    /// <summary>
    /// Mekanin gorunen adidir.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mekanin bulundugu sehirdir.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Mekanin acik adresidir.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Mekanin toplam seyirci kapasitesidir.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Koltuk veya alan yerlesimini JSON formatinda saklar.
    /// </summary>
    public string SeatMapJson { get; private set; } = "{}";

    /// <summary>
    /// Gecerli alanlarla yeni bir mekan aggregate'i olusturur.
    /// </summary>
    /// <param name="name">Mekan adi.</param>
    /// <param name="city">Mekanin bulundugu sehir.</param>
    /// <param name="address">Mekanin acik adresi.</param>
    /// <param name="capacity">Mekanin toplam kapasitesi.</param>
    /// <returns>Yeni mekan aggregate'i.</returns>
    /// <exception cref="DomainException">Zorunlu alanlar bos veya kapasite gecersizse firlatilir.</exception>
    public static Venue Create(string name, string city, string address, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Venue name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainException("Venue city cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Venue address cannot be empty");
        }

        if (capacity <= 0)
        {
            throw new DomainException("Venue capacity must be greater than zero");
        }

        var utcNow = DateTime.UtcNow;

        return new Venue
        {
            Id = Guid.NewGuid(),
            Name = name,
            City = city,
            Address = address,
            Capacity = capacity,
            SeatMapJson = "{}",
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
}

using Biletix.Domain.Base;
using Biletix.Domain.Exceptions;

namespace Biletix.Domain.Entities;

/// <summary>
/// Etkinlikte sahne alan sanatciyi, grubu veya performans sahibini temsil eder.
/// </summary>
public class Performer : AggregateRoot<Guid>
{
    private Performer()
    {
    }

    /// <summary>
    /// Performer'in gorunen adidir.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Performer'in turu veya muzik/etkinlik janridir.
    /// </summary>
    public string Genre { get; private set; } = string.Empty;

    /// <summary>
    /// Performer icin opsiyonel gorsel adresidir.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Gecerli alanlarla yeni bir performer aggregate'i olusturur.
    /// </summary>
    /// <param name="name">Performer adi.</param>
    /// <param name="genre">Performer turu veya janri.</param>
    /// <returns>Yeni performer aggregate'i.</returns>
    /// <exception cref="DomainException">Zorunlu alanlar bos ise firlatilir.</exception>
    public static Performer Create(string name, string genre)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Performer name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(genre))
        {
            throw new DomainException("Performer genre cannot be empty");
        }

        var utcNow = DateTime.UtcNow;

        return new Performer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Genre = genre,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    /// <summary>
    /// Performer icin opsiyonel gorsel adresini gunceller.
    /// </summary>
    /// <param name="imageUrl">Yeni gorsel adresi.</param>
    public void SetImageUrl(string? imageUrl)
    {
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}

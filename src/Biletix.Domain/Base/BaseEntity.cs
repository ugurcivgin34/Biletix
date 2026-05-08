namespace Biletix.Domain.Base;

/// <summary>
/// Domain icindeki tum entity siniflari icin ortak alanlari tasiyan temel siniftir.
/// </summary>
/// <typeparam name="TId">Entity kimliginin tipi.</typeparam>
public abstract class BaseEntity<TId>
{
    /// <summary>
    /// Entity'nin benzersiz kimligidir.
    /// </summary>
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Entity'nin ilk olusturuldugu UTC zamandir.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Entity'nin son guncellendigi UTC zamandir.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete icin kullanilir; true oldugunda kayit veritabaninda kalir ama sorgularda gizlenir.
    /// </summary>
    public bool IsDeleted { get; set; }
}

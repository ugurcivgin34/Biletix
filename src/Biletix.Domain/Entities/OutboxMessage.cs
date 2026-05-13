using Biletix.Domain.Base;

namespace Biletix.Domain.Entities;

/// <summary>
/// Transactional outbox icinde islenecek entegrasyon mesajini temsil eder.
/// </summary>
public class OutboxMessage : BaseEntity<Guid>
{
    /// <summary>
    /// Mesajin olay veya entegrasyon tipi.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Mesajin JSON payload icerigi.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Mesaj basariyla islendiyse true.
    /// </summary>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Mesajin islendigi UTC zaman.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Basarisiz isleme denemesi sayisi.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Son isleme hatasi.
    /// </summary>
    public string? Error { get; set; }
}

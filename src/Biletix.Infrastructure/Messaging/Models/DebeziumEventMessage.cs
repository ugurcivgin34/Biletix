using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biletix.Infrastructure.Messaging.Models;

/// <summary>
/// Debezium unwrap transform sonrasi Events tablosundan gelen CDC mesajini temsil eder.
/// </summary>
public class DebeziumEventMessage
{
    /// <summary>
    /// Etkinlik kimligi.
    /// </summary>
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    /// <summary>
    /// Etkinlik basligi.
    /// </summary>
    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    /// <summary>
    /// Etkinlik aciklamasi.
    /// </summary>
    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Debezium tarafindan numeric veya ISO string formatinda gelebilen baslangic tarihi.
    /// </summary>
    [JsonPropertyName("StartDate")]
    public JsonElement? StartDate { get; set; }

    /// <summary>
    /// Debezium tarafindan numeric veya ISO string formatinda gelebilen bitis tarihi.
    /// </summary>
    [JsonPropertyName("EndDate")]
    public JsonElement? EndDate { get; set; }

    /// <summary>
    /// Etkinligin yayin durumu.
    /// </summary>
    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    /// <summary>
    /// Etkinlik gorsel adresi.
    /// </summary>
    [JsonPropertyName("ImageUrl")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Etkinligin mekan kimligi.
    /// </summary>
    [JsonPropertyName("VenueId")]
    public string? VenueId { get; set; }

    /// <summary>
    /// Etkinligin performer kimligi.
    /// </summary>
    [JsonPropertyName("PerformerId")]
    public string? PerformerId { get; set; }

    /// <summary>
    /// Soft delete durumunu belirtir.
    /// </summary>
    [JsonPropertyName("IsDeleted")]
    public bool? IsDeleted { get; set; }

    /// <summary>
    /// Debezium operasyon kodu: c, u, d veya r.
    /// </summary>
    [JsonPropertyName("__op")]
    public string? Op { get; set; }

    /// <summary>
    /// Kaynak veritabanindaki degisiklik zamani.
    /// </summary>
    [JsonPropertyName("__source_ts_ms")]
    public long? SourceTsMs { get; set; }

    /// <summary>
    /// Mesajin geldigi tablo adi.
    /// </summary>
    [JsonPropertyName("__table")]
    public string? Table { get; set; }

    /// <summary>
    /// Debezium delete rewrite bilgisidir.
    /// </summary>
    [JsonPropertyName("__deleted")]
    public string? Deleted { get; set; }
}

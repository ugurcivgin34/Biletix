using Biletix.Application.Common.Models;

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Etkinlik arama indeksine yazma, silme ve sorgulama operasyonlarini soyutlar.
/// </summary>
public interface IEventSearchService
{
    /// <summary>
    /// Etkinlik dokumanini arama indeksine ekler veya ayni kimlik varsa uzerine yazar.
    /// </summary>
    /// <param name="document">Indekslenecek etkinlik dokumani.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    Task IndexEventAsync(EventSearchDocument document, CancellationToken ct = default);

    /// <summary>
    /// Etkinlik dokumanini arama indeksinde gunceller.
    /// </summary>
    /// <param name="document">Guncellenecek etkinlik dokumani.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    Task UpdateEventAsync(EventSearchDocument document, CancellationToken ct = default);

    /// <summary>
    /// Etkinlik dokumanini arama indeksinden siler.
    /// </summary>
    /// <param name="eventId">Silinecek etkinligin kimligi.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    Task DeleteEventAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Etkinlikleri arama indeksinde filtreler, siralar ve sayfalar.
    /// </summary>
    /// <param name="request">Arama filtreleri ve sayfalama bilgisi.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis etkinlik arama sonucu.</returns>
    Task<PagedResult<EventSearchDocument>> SearchAsync(EventSearchRequest request, CancellationToken ct = default);
}

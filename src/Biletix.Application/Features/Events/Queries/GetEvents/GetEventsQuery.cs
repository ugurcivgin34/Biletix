using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Events.DTOs;

namespace Biletix.Application.Features.Events.Queries.GetEvents;

/// <summary>
/// Etkinlikleri filtreleyerek ve sayfalayarak listelemek icin kullanilan sorgudur.
/// </summary>
public sealed class GetEventsQuery : IQuery<PagedResult<EventSummaryResponse>>
{
    /// <summary>
    /// Etkinlik basliginda aranacak serbest metin.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Mekan sehrine gore filtre.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Etkinlik durumuna gore filtre.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Baslangic tarihi alt siniri.
    /// </summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>
    /// Baslangic tarihi ust siniri.
    /// </summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>
    /// Performer kimligine gore filtre.
    /// </summary>
    public Guid? PerformerId { get; set; }

    /// <summary>
    /// Istenen sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Bir sayfada istenen maksimum etkinlik sayisi.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

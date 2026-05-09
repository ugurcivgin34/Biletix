using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;

namespace Biletix.Application.Features.Events.Queries.SearchEvents;

/// <summary>
/// Elasticsearch uzerinden etkinlik aramasi yapmak icin kullanilan sorgudur.
/// </summary>
public sealed class SearchEventsQuery : IQuery<PagedResult<EventSearchDocument>>
{
    /// <summary>
    /// Baslik, aciklama, performer ve mekan uzerinde aranacak serbest metin.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Mekan sehrine gore filtre.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Performer turune gore filtre.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Etkinlik baslangic tarihi alt siniri.
    /// </summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>
    /// Etkinlik baslangic tarihi ust siniri.
    /// </summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>
    /// Minimum bilet fiyati filtresi.
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maksimum bilet fiyati filtresi.
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Etkinlik durumuna gore filtre.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Siralama alani: startDate, price veya relevance.
    /// </summary>
    public string SortBy { get; set; } = "startDate";

    /// <summary>
    /// Siralamanin azalan yonde yapilip yapilmayacagini belirtir.
    /// </summary>
    public bool SortDescending { get; set; }

    /// <summary>
    /// Istenen sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Bir sayfada istenen maksimum sonuc sayisi.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

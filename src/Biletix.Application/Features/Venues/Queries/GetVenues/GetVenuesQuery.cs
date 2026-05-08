using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Venues.DTOs;

namespace Biletix.Application.Features.Venues.Queries.GetVenues;

/// <summary>
/// Mekanlari filtreleyerek ve sayfalayarak listelemek icin kullanilan sorgudur.
/// </summary>
public sealed class GetVenuesQuery : IQuery<PagedResult<VenueResponse>>
{
    /// <summary>
    /// Mekan adinda aranacak serbest metin.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Mekanlarin filtrelenecegi sehir.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Istenen sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Bir sayfada istenen maksimum mekan sayisi.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Events.DTOs;

namespace Biletix.Application.Features.Events.Queries.GetMyEvents;

/// <summary>
/// Aktif organizer'in kendi etkinliklerini listelemek icin kullanilan sorgudur.
/// </summary>
public sealed class GetMyEventsQuery : IQuery<PagedResult<EventSummaryResponse>>
{
    /// <summary>
    /// Istenen sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Bir sayfada istenen maksimum etkinlik sayisi.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Bookings.DTOs;

namespace Biletix.Application.Features.Bookings.Queries.GetMyBookings;

/// <summary>
/// Aktif kullanicinin rezervasyonlarini listelemek icin kullanilan sorgudur.
/// </summary>
public sealed class GetMyBookingsQuery : IQuery<PagedResult<BookingResponse>>
{
    /// <summary>
    /// Istenen sayfa numarasi.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Bir sayfada istenen maksimum rezervasyon sayisi.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

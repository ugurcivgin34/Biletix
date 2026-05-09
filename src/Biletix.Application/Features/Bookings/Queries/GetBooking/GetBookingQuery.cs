using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Bookings.DTOs;

namespace Biletix.Application.Features.Bookings.Queries.GetBooking;

/// <summary>
/// Tek bir rezervasyon detayini getirmek icin kullanilan sorgudur.
/// </summary>
public sealed class GetBookingQuery : IQuery<BookingResponse>
{
    /// <summary>
    /// Detayi istenen rezervasyon kimligi.
    /// </summary>
    public Guid Id { get; set; }
}

using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Domain.Entities;

namespace Biletix.Application.Features.Bookings;

/// <summary>
/// Booking aggregate'ini API response modellerine donusturur.
/// </summary>
public static class BookingMapper
{
    /// <summary>
    /// Booking aggregate'ini response modeline map eder.
    /// </summary>
    /// <param name="booking">Map edilecek rezervasyon.</param>
    /// <param name="eventTitle">Rezervasyonun ait oldugu etkinlik basligi.</param>
    /// <returns>Rezervasyon cevap modeli.</returns>
    public static BookingResponse ToResponse(this Booking booking, string eventTitle)
    {
        return new BookingResponse(
            booking.Id,
            booking.EventId,
            eventTitle,
            booking.Status,
            booking.TotalAmount,
            booking.ExpiresAt,
            booking.Items.Select(item => new BookingItemResponse(
                item.TicketTypeId,
                item.TicketType?.Name ?? string.Empty,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice)).ToList());
    }

    /// <summary>
    /// Yeni olusturulan booking'i, henuz navigation'lari yuklenmemis item'lar icin event ticket type bilgisiyle map eder.
    /// </summary>
    /// <param name="booking">Map edilecek rezervasyon.</param>
    /// <param name="event">Rezervasyonun ait oldugu etkinlik.</param>
    /// <returns>Rezervasyon cevap modeli.</returns>
    public static BookingResponse ToResponse(this Booking booking, Event @event)
    {
        return new BookingResponse(
            booking.Id,
            booking.EventId,
            @event.Title,
            booking.Status,
            booking.TotalAmount,
            booking.ExpiresAt,
            booking.Items.Select(item =>
            {
                var ticketType = @event.TicketTypes.FirstOrDefault(type => type.Id == item.TicketTypeId);

                return new BookingItemResponse(
                    item.TicketTypeId,
                    ticketType?.Name ?? string.Empty,
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice);
            }).ToList());
    }
}

using Biletix.Application.Features.Events.DTOs;
using Biletix.Domain.Entities;

namespace Biletix.Application.Features.Events.Mappers;

/// <summary>
/// Event ve TicketType domain nesnelerini API cevap modellerine donusturur.
/// </summary>
public static class EventMapper
{
    /// <summary>
    /// Etkinlik aggregate'ini detay cevap modeline map eder.
    /// </summary>
    /// <param name="event">Map edilecek etkinlik aggregate'i.</param>
    /// <returns>Etkinlik detay cevap modeli.</returns>
    public static EventResponse ToResponse(this Event @event)
    {
        return new EventResponse(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.StartDate,
            @event.EndDate,
            @event.Status.ToString(),
            @event.ImageUrl,
            @event.VenueId,
            @event.Venue?.Name ?? string.Empty,
            @event.Venue?.City ?? string.Empty,
            @event.Venue?.Capacity ?? 0,
            @event.PerformerId,
            @event.Performer?.Name ?? string.Empty,
            @event.Performer?.Genre ?? string.Empty,
            @event.TicketTypes.Select(ticketType => ticketType.ToResponse()).ToList(),
            @event.CreatedAt);
    }

    /// <summary>
    /// Etkinlik aggregate'ini listeleme icin hafif cevap modeline map eder.
    /// </summary>
    /// <param name="event">Map edilecek etkinlik aggregate'i.</param>
    /// <returns>Etkinlik ozet cevap modeli.</returns>
    public static EventSummaryResponse ToSummaryResponse(this Event @event)
    {
        var minPrice = @event.TicketTypes.Count == 0
            ? 0
            : @event.TicketTypes.Min(ticketType => ticketType.Price);

        var totalAvailableTickets = @event.TicketTypes.Sum(ticketType => ticketType.AvailableCount);

        return new EventSummaryResponse(
            @event.Id,
            @event.Title,
            @event.StartDate,
            @event.Status.ToString(),
            @event.Venue?.Name ?? string.Empty,
            @event.Venue?.City ?? string.Empty,
            @event.Performer?.Name ?? string.Empty,
            minPrice,
            totalAvailableTickets);
    }

    /// <summary>
    /// Bilet tipi entity'sini cevap modeline map eder.
    /// </summary>
    /// <param name="ticketType">Map edilecek bilet tipi.</param>
    /// <returns>Bilet tipi cevap modeli.</returns>
    public static TicketTypeResponse ToResponse(this TicketType ticketType)
    {
        return new TicketTypeResponse(
            ticketType.Id,
            ticketType.Name,
            ticketType.Price,
            ticketType.TotalCapacity,
            ticketType.SoldCount,
            ticketType.ReservedCount,
            ticketType.AvailableCount);
    }
}

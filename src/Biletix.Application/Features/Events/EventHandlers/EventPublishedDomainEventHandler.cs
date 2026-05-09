using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Domain.Entities;
using Biletix.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.EventHandlers;

/// <summary>
/// Etkinlik yayina alindiginda arama indeksine yazilmasini saglar.
/// </summary>
public sealed class EventPublishedDomainEventHandler : INotificationHandler<EventPublishedDomainEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventSearchService _eventSearchService;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani ve arama servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="eventSearchService">Etkinlik arama servisi.</param>
    public EventPublishedDomainEventHandler(
        IApplicationDbContext context,
        IEventSearchService eventSearchService)
    {
        _context = context;
        _eventSearchService = eventSearchService;
    }

    /// <summary>
    /// Yayina alinan etkinligi iliskileriyle okur ve Elasticsearch dokumanina map ederek indeksler.
    /// </summary>
    /// <param name="notification">Etkinlik yayina alma domain event'i.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(EventPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .Include(item => item.Venue)
            .Include(item => item.Performer)
            .Include(item => item.TicketTypes)
            .FirstOrDefaultAsync(item => item.Id == notification.EventId, cancellationToken);

        if (@event is null)
        {
            return;
        }

        await _eventSearchService.IndexEventAsync(ToSearchDocument(@event), cancellationToken);
    }

    private static EventSearchDocument ToSearchDocument(Event @event)
    {
        return new EventSearchDocument
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartDate = @event.StartDate,
            EndDate = @event.EndDate,
            Status = @event.Status.ToString(),
            ImageUrl = @event.ImageUrl,
            VenueId = @event.VenueId,
            VenueName = @event.Venue?.Name ?? string.Empty,
            VenueCity = @event.Venue?.City ?? string.Empty,
            VenueCapacity = @event.Venue?.Capacity ?? 0,
            PerformerId = @event.PerformerId,
            PerformerName = @event.Performer?.Name ?? string.Empty,
            PerformerGenre = @event.Performer?.Genre ?? string.Empty,
            MinPrice = @event.TicketTypes.Count == 0 ? 0 : @event.TicketTypes.Min(ticketType => ticketType.Price),
            TotalAvailableTickets = @event.TicketTypes.Sum(ticketType => ticketType.AvailableCount),
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.UpdatedAt
        };
    }
}

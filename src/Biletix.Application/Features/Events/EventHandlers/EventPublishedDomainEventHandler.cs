using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Domain.Entities;
using Biletix.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Events.EventHandlers;

/// <summary>
/// Etkinlik yayina alindiginda arama indeksini gunceller.
/// </summary>
public sealed class EventPublishedDomainEventHandler : INotificationHandler<EventPublishedDomainEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventSearchService _eventSearchService;
    private readonly ILogger<EventPublishedDomainEventHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu bagimliliklari alir.
    /// </summary>
    /// <param name="context">Etkinlik bilgisini okumak icin kullanilan veritabani baglami.</param>
    /// <param name="eventSearchService">Arama indeksine yazan servis.</param>
    /// <param name="logger">Domain event loglarini yazan logger.</param>
    public EventPublishedDomainEventHandler(
        IApplicationDbContext context,
        IEventSearchService eventSearchService,
        ILogger<EventPublishedDomainEventHandler> logger)
    {
        _context = context;
        _eventSearchService = eventSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Etkinligi Elasticsearch'e yazar.
    /// </summary>
    /// <param name="notification">Etkinlik yayina alma domain event'i.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(EventPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .Include(item => item.Venue)
            .Include(item => item.Performer)
            .Include(item => item.TicketTypes)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == notification.EventId, cancellationToken);

        if (@event is null)
        {
            _logger.LogWarning("Published event {Id} could not be found for ES sync", notification.EventId);
            return;
        }

        if (@event.Status != EventStatus.Published)
        {
            await _eventSearchService.DeleteEventAsync(notification.EventId, cancellationToken);
            return;
        }

        await _eventSearchService.IndexEventAsync(ToSearchDocument(@event), cancellationToken);

        _logger.LogInformation(
            "Event {Id} published and synced to Elasticsearch",
            notification.EventId);
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
            MinPrice = @event.TicketTypes.Any() ? @event.TicketTypes.Min(ticketType => ticketType.Price) : 0,
            TotalAvailableTickets = @event.TicketTypes.Sum(ticketType => ticketType.AvailableCount),
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.UpdatedAt
        };
    }
}

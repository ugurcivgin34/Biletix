using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Events.EventHandlers;

/// <summary>
/// Etkinlik iptal edildiginde arama indeksinden siler.
/// </summary>
public sealed class EventCancelledDomainEventHandler : INotificationHandler<EventCancelledDomainEvent>
{
    private readonly IEventSearchService _eventSearchService;
    private readonly ILogger<EventCancelledDomainEventHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu bagimliliklari alir.
    /// </summary>
    /// <param name="eventSearchService">Arama indeksinden silen servis.</param>
    /// <param name="logger">Domain event loglarini yazan logger.</param>
    public EventCancelledDomainEventHandler(
        IEventSearchService eventSearchService,
        ILogger<EventCancelledDomainEventHandler> logger)
    {
        _eventSearchService = eventSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Etkinligi Elasticsearch'ten siler.
    /// </summary>
    /// <param name="notification">Etkinlik iptal domain event'i.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(EventCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        await _eventSearchService.DeleteEventAsync(notification.EventId, cancellationToken);

        _logger.LogInformation(
            "Event {Id} cancelled and removed from Elasticsearch",
            notification.EventId);
    }
}

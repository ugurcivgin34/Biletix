using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Events;
using MediatR;

namespace Biletix.Application.Features.Events.EventHandlers;

/// <summary>
/// Etkinlik iptal edildiginde arama indeksinden kaldirilmasini saglar.
/// </summary>
public sealed class EventCancelledDomainEventHandler : INotificationHandler<EventCancelledDomainEvent>
{
    private readonly IEventSearchService _eventSearchService;

    /// <summary>
    /// Handler'in ihtiyac duydugu arama servisini alir.
    /// </summary>
    /// <param name="eventSearchService">Etkinlik arama servisi.</param>
    public EventCancelledDomainEventHandler(IEventSearchService eventSearchService)
    {
        _eventSearchService = eventSearchService;
    }

    /// <summary>
    /// Iptal edilen etkinligi arama sonucundan cikarmak icin indeks dokumanini siler.
    /// </summary>
    /// <param name="notification">Etkinlik iptal domain event'i.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public Task Handle(EventCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        return _eventSearchService.DeleteEventAsync(notification.EventId, cancellationToken);
    }
}

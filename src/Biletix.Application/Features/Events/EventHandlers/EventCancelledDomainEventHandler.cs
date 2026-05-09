using Biletix.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Events.EventHandlers;

/// <summary>
/// Etkinlik iptal edildiginde manuel Elasticsearch silme yerine CDC akisini loglar.
/// </summary>
public sealed class EventCancelledDomainEventHandler : INotificationHandler<EventCancelledDomainEvent>
{
    private readonly ILogger<EventCancelledDomainEventHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu logger bagimliligini alir.
    /// </summary>
    /// <param name="logger">Domain event loglarini yazan logger.</param>
    public EventCancelledDomainEventHandler(ILogger<EventCancelledDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Etkinligin iptal edildigini loglar; Elasticsearch senkronizasyonunu CDC consumer ustlenir.
    /// </summary>
    /// <param name="notification">Etkinlik iptal domain event'i.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public Task Handle(EventCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event {Id} cancelled - ES sync handled by CDC",
            notification.EventId);

        return Task.CompletedTask;
    }
}

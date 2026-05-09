using Biletix.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Events.EventHandlers;

/// <summary>
/// Etkinlik yayina alindiginda manuel Elasticsearch senkronizasyonu yerine CDC akisini loglar.
/// </summary>
public sealed class EventPublishedDomainEventHandler : INotificationHandler<EventPublishedDomainEvent>
{
    private readonly ILogger<EventPublishedDomainEventHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu logger bagimliligini alir.
    /// </summary>
    /// <param name="logger">Domain event loglarini yazan logger.</param>
    public EventPublishedDomainEventHandler(ILogger<EventPublishedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Etkinligin yayina alindigini loglar; Elasticsearch senkronizasyonunu CDC consumer ustlenir.
    /// </summary>
    /// <param name="notification">Etkinlik yayina alma domain event'i.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public Task Handle(EventPublishedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event {Id} published - ES sync handled by CDC",
            notification.EventId);

        return Task.CompletedTask;
    }
}

using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Bir etkinlik yayina alindiginda yayinlanan domain event'tir.
/// </summary>
/// <param name="EventId">Yayina alinan etkinligin kimligi.</param>
public sealed record EventPublishedDomainEvent(Guid EventId) : IDomainEvent;

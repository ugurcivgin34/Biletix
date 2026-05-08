using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Bir etkinlik iptal edildiginde yayinlanan domain event'tir.
/// </summary>
/// <param name="EventId">Iptal edilen etkinligin kimligi.</param>
public sealed record EventCancelledDomainEvent(Guid EventId) : IDomainEvent;

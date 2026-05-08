using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Yeni bir etkinlik olusturuldugunda yayinlanan domain event'tir.
/// </summary>
/// <param name="EventId">Olusturulan etkinligin kimligi.</param>
public sealed record EventCreatedDomainEvent(Guid EventId) : IDomainEvent;

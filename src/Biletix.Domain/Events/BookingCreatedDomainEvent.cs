using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Yeni bir rezervasyon olusturuldugunda yayinlanan domain event'tir.
/// </summary>
/// <param name="BookingId">Olusturulan rezervasyonun kimligi.</param>
/// <param name="UserId">Rezervasyonu yapan kullanicinin kimligi.</param>
public sealed record BookingCreatedDomainEvent(Guid BookingId, Guid UserId) : IDomainEvent;

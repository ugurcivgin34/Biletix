using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Bir rezervasyon iptal edildiginde yayinlanan domain event'tir.
/// </summary>
/// <param name="BookingId">Iptal edilen rezervasyonun kimligi.</param>
/// <param name="UserId">Rezervasyonu yapan kullanicinin kimligi.</param>
public sealed record BookingCancelledDomainEvent(Guid BookingId, Guid UserId) : IDomainEvent;

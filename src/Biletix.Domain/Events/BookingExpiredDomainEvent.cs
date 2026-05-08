using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Bir rezervasyonun suresi doldugunda yayinlanan domain event'tir.
/// </summary>
/// <param name="BookingId">Suresi dolan rezervasyonun kimligi.</param>
/// <param name="UserId">Rezervasyonu yapan kullanicinin kimligi.</param>
public sealed record BookingExpiredDomainEvent(Guid BookingId, Guid UserId) : IDomainEvent;

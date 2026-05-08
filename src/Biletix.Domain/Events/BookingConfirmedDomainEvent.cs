using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// Bir rezervasyon kesinlestiginde yayinlanan domain event'tir.
/// </summary>
/// <param name="BookingId">Kesinlesen rezervasyonun kimligi.</param>
/// <param name="UserId">Rezervasyonu yapan kullanicinin kimligi.</param>
/// <param name="EventId">Rezervasyonun ait oldugu etkinlik kimligi.</param>
public sealed record BookingConfirmedDomainEvent(Guid BookingId, Guid UserId, Guid EventId) : IDomainEvent;

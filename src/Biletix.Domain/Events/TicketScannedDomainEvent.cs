using Biletix.Domain.Base;

namespace Biletix.Domain.Events;

/// <summary>
/// QR bilet taramasi tamamlandiginda olusan domain event'tir.
/// </summary>
public sealed record TicketScannedDomainEvent(
    Guid BookingId,
    Guid EventId,
    bool IsValid) : IDomainEvent;

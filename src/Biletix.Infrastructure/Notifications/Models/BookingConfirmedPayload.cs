namespace Biletix.Infrastructure.Notifications.Models;

/// <summary>
/// booking.confirmed integration event payload modelidir.
/// </summary>
public sealed record BookingConfirmedPayload(
    Guid BookingId,
    Guid UserId,
    Guid EventId,
    decimal TotalAmount,
    IList<BookingItemPayload> Items);

/// <summary>
/// Rezervasyon kalemi payload modelidir.
/// </summary>
public sealed record BookingItemPayload(
    string? TicketTypeName,
    int Quantity,
    decimal UnitPrice);

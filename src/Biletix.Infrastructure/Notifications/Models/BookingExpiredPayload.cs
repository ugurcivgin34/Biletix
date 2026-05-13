namespace Biletix.Infrastructure.Notifications.Models;

/// <summary>
/// booking.expired integration event payload modelidir.
/// </summary>
public sealed record BookingExpiredPayload(
    Guid BookingId,
    Guid UserId,
    DateTime ExpiredAt);

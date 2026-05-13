namespace Biletix.Infrastructure.Notifications.Models;

/// <summary>
/// booking.payment_failed integration event payload modelidir.
/// </summary>
public sealed record BookingPaymentFailedPayload(
    Guid BookingId,
    Guid UserId);

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// QR bilet token'i ve PNG QR kod uretimi icin servis sozlesmesidir.
/// </summary>
public interface IQrTicketService
{
    /// <summary>
    /// Rezervasyon, kullanici ve etkinlik bilgilerini iceren JWT imzali bilet token'i uretir.
    /// </summary>
    string GenerateTicketToken(Guid bookingId, Guid userId, Guid eventId);

    /// <summary>
    /// Bilet token'ini dogrular ve token claim'lerini dondurur; gecersizse null dondurur.
    /// </summary>
    QrTicketClaims? ValidateTicketToken(string token);

    /// <summary>
    /// Token degerinden PNG formatinda QR kod byte dizisi uretir.
    /// </summary>
    byte[] GenerateQrCodePng(string token);
}

/// <summary>
/// Dogrulanmis QR bilet token claim'lerini temsil eder.
/// </summary>
public sealed record QrTicketClaims(
    Guid BookingId,
    Guid UserId,
    Guid EventId,
    DateTime IssuedAt,
    DateTime ExpiresAt);

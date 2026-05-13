using Biletix.Application.Features.Bookings.Commands.ReserveTickets;
using Biletix.Domain.Enums;

namespace Biletix.Application.Features.Bookings.Saga;

/// <summary>
/// Rezervasyon ve odeme niyeti olusturma adimlarini tek checkout akisi olarak orkestre eder.
/// </summary>
public interface IBookingSaga
{
    /// <summary>
    /// Bilet rezervasyonu ve payment intent olusturma adimlarini calistirir.
    /// </summary>
    /// <param name="eventId">Rezervasyon yapilacak etkinlik kimligi.</param>
    /// <param name="userId">Checkout yapan kullanici kimligi.</param>
    /// <param name="items">Rezerve edilecek bilet kalemleri.</param>
    /// <param name="idempotencyKey">Checkout idempotency anahtari.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    /// <returns>Saga sonucu.</returns>
    Task<BookingSagaResult> ExecuteAsync(
        Guid eventId,
        Guid userId,
        List<ReserveTicketItemDto> items,
        string idempotencyKey,
        CancellationToken ct = default);
}

/// <summary>
/// Checkout saga sonucunu temsil eder.
/// </summary>
/// <param name="IsSuccess">Saga basariyla tamamlandiysa true.</param>
/// <param name="BookingId">Olusan rezervasyon kimligi.</param>
/// <param name="ClientSecret">Frontend odeme tamamlamasi icin client secret.</param>
/// <param name="Error">Saga basarisizsa hata mesaji.</param>
/// <param name="FinalState">Saga'nin son durumu.</param>
public sealed record BookingSagaResult(
    bool IsSuccess,
    Guid? BookingId,
    string? ClientSecret,
    string? Error,
    BookingSagaState FinalState);

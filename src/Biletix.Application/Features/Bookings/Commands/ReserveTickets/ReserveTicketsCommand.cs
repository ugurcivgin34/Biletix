using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Bookings.DTOs;

namespace Biletix.Application.Features.Bookings.Commands.ReserveTickets;

/// <summary>
/// Yayindaki bir etkinlik icin bilet rezervasyonu olusturmak icin kullanilan komuttur.
/// </summary>
public sealed class ReserveTicketsCommand : ICommand<BookingResponse>
{
    /// <summary>
    /// Komutu bos olusturur.
    /// </summary>
    public ReserveTicketsCommand()
    {
    }

    /// <summary>
    /// Komutu etkinlik, bilet kalemleri ve idempotency key ile olusturur.
    /// </summary>
    /// <param name="eventId">Rezervasyon yapilacak etkinlik kimligi.</param>
    /// <param name="items">Rezerve edilecek bilet kalemleri.</param>
    /// <param name="idempotencyKey">Idempotency anahtari.</param>
    public ReserveTicketsCommand(
        Guid eventId,
        List<ReserveTicketItemDto> items,
        string idempotencyKey)
    {
        EventId = eventId;
        Items = items;
        IdempotencyKey = idempotencyKey;
    }

    /// <summary>
    /// Rezervasyon yapilacak etkinlik kimligi.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Rezerve edilecek bilet kalemleri.
    /// </summary>
    public List<ReserveTicketItemDto> Items { get; set; } = new();

    /// <summary>
    /// Ayni istegin tekrar islenmesini engelleyen idempotency anahtari.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}

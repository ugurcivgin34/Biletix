using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Bookings.DTOs;

namespace Biletix.Application.Features.Bookings.Commands.ReserveTickets;

/// <summary>
/// Yayindaki bir etkinlik icin bilet rezervasyonu olusturmak icin kullanilan komuttur.
/// </summary>
public sealed class ReserveTicketsCommand : ICommand<BookingResponse>
{
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

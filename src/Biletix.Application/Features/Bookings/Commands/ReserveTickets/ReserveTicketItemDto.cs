namespace Biletix.Application.Features.Bookings.Commands.ReserveTickets;

/// <summary>
/// Rezervasyon istegindeki tek bir bilet tipi ve adet bilgisidir.
/// </summary>
/// <param name="TicketTypeId">Rezerve edilecek bilet tipi kimligi.</param>
/// <param name="Quantity">Rezerve edilecek bilet adedi.</param>
public sealed record ReserveTicketItemDto(Guid TicketTypeId, int Quantity);

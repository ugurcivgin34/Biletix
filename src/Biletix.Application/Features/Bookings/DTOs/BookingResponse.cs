using Biletix.Domain.Entities;

namespace Biletix.Application.Features.Bookings.DTOs;

/// <summary>
/// Rezervasyon olusturma ve okuma endpoint'lerinde dondurulen cevap modelidir.
/// </summary>
/// <param name="Id">Rezervasyon kimligi.</param>
/// <param name="EventId">Rezervasyonun ait oldugu etkinlik kimligi.</param>
/// <param name="EventTitle">Rezervasyonun ait oldugu etkinlik basligi.</param>
/// <param name="Status">Rezervasyon durumu.</param>
/// <param name="TotalAmount">Rezervasyon toplam tutari.</param>
/// <param name="ExpiresAt">Pending rezervasyonun gecerlilik bitis zamani.</param>
/// <param name="Items">Rezervasyon kalemleri.</param>
public sealed record BookingResponse(
    Guid Id,
    Guid EventId,
    string EventTitle,
    BookingStatus Status,
    decimal TotalAmount,
    DateTime? ExpiresAt,
    IReadOnlyList<BookingItemResponse> Items);

/// <summary>
/// Rezervasyon icindeki tek bir bilet kalemini temsil eden cevap modelidir.
/// </summary>
/// <param name="TicketTypeId">Bilet tipi kimligi.</param>
/// <param name="TicketTypeName">Bilet tipi adi.</param>
/// <param name="Quantity">Bilet adedi.</param>
/// <param name="UnitPrice">Birim fiyat.</param>
/// <param name="TotalPrice">Kalem toplam fiyati.</param>
public sealed record BookingItemResponse(
    Guid TicketTypeId,
    string TicketTypeName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

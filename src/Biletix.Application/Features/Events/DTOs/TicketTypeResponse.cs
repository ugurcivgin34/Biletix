namespace Biletix.Application.Features.Events.DTOs;

/// <summary>
/// Etkinlik detayinda dondurulen bilet tipi cevap modelidir.
/// </summary>
/// <param name="Id">Bilet tipinin benzersiz kimligi.</param>
/// <param name="Name">Bilet tipinin gorunen adi.</param>
/// <param name="Price">Bilet tipinin birim fiyati.</param>
/// <param name="TotalCapacity">Bilet tipi icin toplam kapasite.</param>
/// <param name="SoldCount">Satilmis bilet sayisi.</param>
/// <param name="ReservedCount">Rezerve edilmis bilet sayisi.</param>
/// <param name="AvailableCount">Satis veya rezervasyon icin uygun bilet sayisi.</param>
public sealed record TicketTypeResponse(
    Guid Id,
    string Name,
    decimal Price,
    int TotalCapacity,
    int SoldCount,
    int ReservedCount,
    int AvailableCount);

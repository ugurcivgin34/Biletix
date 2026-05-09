namespace Biletix.Application.Features.Events.DTOs;

/// <summary>
/// Etkinlik listeleme endpoint'inde dondurulen hafif cevap modelidir.
/// </summary>
/// <param name="Id">Etkinligin benzersiz kimligi.</param>
/// <param name="Title">Etkinligin gorunen basligi.</param>
/// <param name="StartDate">Etkinligin baslangic tarihi.</param>
/// <param name="Status">Etkinligin yayin durumu.</param>
/// <param name="VenueName">Etkinligin mekan adi.</param>
/// <param name="VenueCity">Etkinligin mekan sehri.</param>
/// <param name="PerformerName">Etkinligin performer adi.</param>
/// <param name="MinPrice">Etkinlikteki en dusuk bilet fiyati.</param>
/// <param name="TotalAvailableTickets">Etkinlikteki toplam uygun bilet sayisi.</param>
public sealed record EventSummaryResponse(
    Guid Id,
    string Title,
    DateTime StartDate,
    string Status,
    string VenueName,
    string VenueCity,
    string PerformerName,
    decimal MinPrice,
    int TotalAvailableTickets);

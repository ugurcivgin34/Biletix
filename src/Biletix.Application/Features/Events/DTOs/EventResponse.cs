namespace Biletix.Application.Features.Events.DTOs;

/// <summary>
/// Etkinlik detay endpoint'inde dondurulen tam cevap modelidir.
/// </summary>
/// <param name="Id">Etkinligin benzersiz kimligi.</param>
/// <param name="Title">Etkinligin gorunen basligi.</param>
/// <param name="Description">Etkinligin detay aciklamasi.</param>
/// <param name="StartDate">Etkinligin baslangic tarihi.</param>
/// <param name="EndDate">Etkinligin bitis tarihi.</param>
/// <param name="Status">Etkinligin yayin durumu.</param>
/// <param name="ImageUrl">Etkinlik gorsel adresi.</param>
/// <param name="VenueId">Etkinligin mekan kimligi.</param>
/// <param name="VenueName">Etkinligin mekan adi.</param>
/// <param name="VenueCity">Etkinligin mekan sehri.</param>
/// <param name="VenueCapacity">Etkinligin mekan kapasitesi.</param>
/// <param name="PerformerId">Etkinligin performer kimligi.</param>
/// <param name="PerformerName">Etkinligin performer adi.</param>
/// <param name="PerformerGenre">Etkinligin performer turu.</param>
/// <param name="TicketTypes">Etkinlige ait bilet tipleri.</param>
/// <param name="CreatedAt">Etkinligin olusturulma zamani.</param>
public sealed record EventResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    string? ImageUrl,
    Guid VenueId,
    string VenueName,
    string VenueCity,
    int VenueCapacity,
    Guid PerformerId,
    string PerformerName,
    string PerformerGenre,
    IReadOnlyList<TicketTypeResponse> TicketTypes,
    DateTime CreatedAt);

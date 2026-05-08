namespace Biletix.Application.Features.Venues.DTOs;

/// <summary>
/// Mekan listeleme ve detay endpoint'lerinde dondurulen cevap modelidir.
/// </summary>
/// <param name="Id">Mekanin benzersiz kimligi.</param>
/// <param name="Name">Mekanin gorunen adi.</param>
/// <param name="City">Mekanin bulundugu sehir.</param>
/// <param name="Address">Mekanin acik adresi.</param>
/// <param name="Capacity">Mekanin toplam kapasitesi.</param>
/// <param name="CreatedAt">Mekanin olusturulma zamani.</param>
public sealed record VenueResponse(
    Guid Id,
    string Name,
    string City,
    string Address,
    int Capacity,
    DateTime CreatedAt);

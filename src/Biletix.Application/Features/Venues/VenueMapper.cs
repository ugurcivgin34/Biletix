using Biletix.Application.Features.Venues.DTOs;
using Biletix.Domain.Entities;

namespace Biletix.Application.Features.Venues;

/// <summary>
/// Venue domain nesnesini dis dunyaya donulecek cevap modellerine donusturur.
/// </summary>
public static class VenueMapper
{
    /// <summary>
    /// Venue entity'sini VenueResponse DTO'suna map eder.
    /// </summary>
    /// <param name="venue">Map edilecek mekan entity'si.</param>
    /// <returns>Mekan cevap modeli.</returns>
    public static VenueResponse ToResponse(this Venue venue)
    {
        return new VenueResponse(
            venue.Id,
            venue.Name,
            venue.City,
            venue.Address,
            venue.Capacity,
            venue.CreatedAt);
    }
}

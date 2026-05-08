using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Venues.DTOs;

namespace Biletix.Application.Features.Venues.Queries.GetVenue;

/// <summary>
/// Tek bir mekan detayini getirmek icin kullanilan sorgudur.
/// </summary>
public sealed class GetVenueQuery : IQuery<VenueResponse>
{
    /// <summary>
    /// Detayi istenen mekanin kimligi.
    /// </summary>
    public Guid Id { get; set; }
}

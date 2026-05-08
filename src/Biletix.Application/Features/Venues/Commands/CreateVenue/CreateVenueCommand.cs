using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Venues.Commands.CreateVenue;

/// <summary>
/// Yeni bir mekan olusturmak icin kullanilan CQRS komutudur.
/// </summary>
public sealed class CreateVenueCommand : ICommand<Guid>
{
    /// <summary>
    /// Olusturulacak mekanin gorunen adi.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Olusturulacak mekanin bulundugu sehir.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Olusturulacak mekanin acik adresi.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Olusturulacak mekanin toplam kapasitesi.
    /// </summary>
    public int Capacity { get; set; }
}

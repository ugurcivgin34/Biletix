using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Venues.Commands.UpdateVenue;

/// <summary>
/// Var olan bir mekanin temel bilgilerini guncellemek icin kullanilan komuttur.
/// </summary>
public sealed class UpdateVenueCommand : ICommand
{
    /// <summary>
    /// Guncellenecek mekanin kimligi.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mekanin yeni gorunen adi.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mekanin yeni sehir bilgisi.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Mekanin yeni acik adresi.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Mekanin yeni toplam kapasitesi.
    /// </summary>
    public int Capacity { get; set; }
}

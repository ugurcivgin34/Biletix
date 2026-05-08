using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Venues.Commands.DeleteVenue;

/// <summary>
/// Bir mekani soft delete ile silmek icin kullanilan komuttur.
/// </summary>
public sealed class DeleteVenueCommand : ICommand
{
    /// <summary>
    /// Silinecek mekanin kimligi.
    /// </summary>
    public Guid Id { get; set; }
}

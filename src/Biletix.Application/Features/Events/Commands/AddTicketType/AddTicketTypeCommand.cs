using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Events.Commands.AddTicketType;

/// <summary>
/// Var olan bir etkinlige yeni bilet tipi eklemek icin kullanilan komuttur.
/// </summary>
public sealed class AddTicketTypeCommand : ICommand<Guid>
{
    /// <summary>
    /// Bilet tipi eklenecek etkinligin kimligi.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Yeni bilet tipinin gorunen adi.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Yeni bilet tipinin birim fiyati.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Yeni bilet tipi icin toplam kapasite.
    /// </summary>
    public int TotalCapacity { get; set; }
}

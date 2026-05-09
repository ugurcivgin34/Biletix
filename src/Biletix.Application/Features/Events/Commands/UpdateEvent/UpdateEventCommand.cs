using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Events.Commands.UpdateEvent;

/// <summary>
/// Draft durumundaki bir etkinligin temel bilgilerini guncellemek icin kullanilan komuttur.
/// </summary>
public sealed class UpdateEventCommand : ICommand
{
    /// <summary>
    /// Guncellenecek etkinligin kimligi.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Etkinligin yeni basligi.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Etkinligin yeni aciklamasi.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Etkinligin yeni baslangic tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Etkinligin yeni bitis tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Etkinligin yeni opsiyonel gorsel adresi.
    /// </summary>
    public string? ImageUrl { get; set; }
}

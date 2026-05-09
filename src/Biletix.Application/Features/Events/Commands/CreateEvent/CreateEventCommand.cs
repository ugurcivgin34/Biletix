using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Yeni bir etkinlik ve ilk bilet tiplerini olusturmak icin kullanilan komuttur.
/// </summary>
public sealed class CreateEventCommand : ICommand<Guid>
{
    /// <summary>
    /// Etkinligin gorunen basligi.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Etkinligin detay aciklamasi.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Etkinligin baslangic tarihi.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Etkinligin bitis tarihi.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Etkinligin yapilacagi mekan kimligi.
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Etkinlikte yer alacak performer kimligi.
    /// </summary>
    public Guid PerformerId { get; set; }

    /// <summary>
    /// Etkinlik icin opsiyonel gorsel adresi.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Etkinlik olusturulurken eklenecek bilet tipleri.
    /// </summary>
    public List<CreateTicketTypeDto> TicketTypes { get; set; } = new();
}

using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Events.Commands.CancelEvent;

/// <summary>
/// Bir etkinligi iptal etmek icin kullanilan komuttur.
/// </summary>
public sealed class CancelEventCommand : ICommand
{
    /// <summary>
    /// Iptal edilecek etkinligin kimligi.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Iptal gerekcesi.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

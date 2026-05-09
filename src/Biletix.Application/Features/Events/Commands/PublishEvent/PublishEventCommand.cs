using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Events.Commands.PublishEvent;

/// <summary>
/// Draft durumundaki bir etkinligi yayina almak icin kullanilan komuttur.
/// </summary>
public sealed class PublishEventCommand : ICommand
{
    /// <summary>
    /// Yayina alinacak etkinligin kimligi.
    /// </summary>
    public Guid Id { get; set; }
}

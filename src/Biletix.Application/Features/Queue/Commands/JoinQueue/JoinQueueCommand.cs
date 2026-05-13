using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Queue.DTOs;

namespace Biletix.Application.Features.Queue.Commands.JoinQueue;

/// <summary>
/// Aktif kullaniciyi etkinlik bekleme sirasina eklemek icin kullanilan komuttur.
/// </summary>
public sealed class JoinQueueCommand : ICommand<QueueStatusResponse>
{
    /// <summary>
    /// Siraya girilecek etkinlik kimligi.
    /// </summary>
    public Guid EventId { get; set; }
}

using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Queue.DTOs;

namespace Biletix.Application.Features.Queue.Queries.GetQueueStatus;

/// <summary>
/// Aktif kullanicinin etkinlik bekleme sirasi durumunu getirmek icin kullanilan sorgudur.
/// </summary>
public sealed class GetQueueStatusQuery : IQuery<QueueStatusResponse>
{
    /// <summary>
    /// Sira durumu sorgulanacak etkinlik kimligi.
    /// </summary>
    public Guid EventId { get; set; }
}

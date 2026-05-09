using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Events.DTOs;

namespace Biletix.Application.Features.Events.Queries.GetEvent;

/// <summary>
/// Tek bir etkinlik detayini getirmek icin kullanilan sorgudur.
/// </summary>
public sealed class GetEventQuery : IQuery<EventResponse>
{
    /// <summary>
    /// Detayi istenen etkinligin kimligi.
    /// </summary>
    public Guid Id { get; set; }
}

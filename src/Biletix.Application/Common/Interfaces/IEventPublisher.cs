namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Entegrasyon event'lerini dis mesajlasma altyapisina yayinlayan servis sozlesmesidir.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Belirtilen topic'e event payload'unu yayinlar.
    /// </summary>
    /// <param name="topic">Mesajin yayinlanacagi topic.</param>
    /// <param name="eventType">Event tipi.</param>
    /// <param name="payload">Yayinlanacak mesaj icerigi.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    Task PublishAsync(
        string topic,
        string eventType,
        string payload,
        CancellationToken ct = default);
}

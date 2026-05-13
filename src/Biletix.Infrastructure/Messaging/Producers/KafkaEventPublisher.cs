using System.Text;
using Biletix.Application.Common.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Biletix.Infrastructure.Messaging.Producers;

/// <summary>
/// Entegrasyon event'lerini Kafka topic'lerine yayinlayan producer implementasyonudur.
/// </summary>
public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    /// <summary>
    /// Kafka producer konfigurasyonunu olusturur.
    /// </summary>
    /// <param name="configuration">Kafka baglanti ayarlarini tasiyan konfigurasyon.</param>
    /// <param name="logger">Producer loglarini yazan logger.</param>
    public KafkaEventPublisher(
        IConfiguration configuration,
        ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <inheritdoc />
    public async Task PublishAsync(
        string topic,
        string eventType,
        string payload,
        CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            Key = eventType,
            Value = payload,
            Headers = new Headers
            {
                { "eventType", Encoding.UTF8.GetBytes(eventType) },
                { "publishedAt", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
            }
        };

        var result = await _producer.ProduceAsync(topic, message, ct);

        _logger.LogInformation(
            "Published {EventType} to {Topic} partition {Partition} offset {Offset}",
            eventType,
            topic,
            result.Partition,
            result.Offset);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}

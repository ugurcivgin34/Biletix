using Biletix.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biletix.Infrastructure.Messaging.Workers;

/// <summary>
/// OutboxMessages tablosundaki islenmemis mesajlari Kafka topic'lerine yayinlayan background worker'dir.
/// </summary>
public sealed class OutboxRelayWorker : BackgroundService
{
    private const int BatchSize = 50;
    private const int DelaySeconds = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxRelayWorker> _logger;

    /// <summary>
    /// Worker'in ihtiyac duydugu scope factory ve logger servislerini alir.
    /// </summary>
    /// <param name="scopeFactory">Scoped servisleri batch bazinda olusturmak icin kullanilir.</param>
    /// <param name="logger">Worker loglarini yazan logger.</param>
    public OutboxRelayWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxRelayWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Worker yasam dongusu boyunca periyodik olarak outbox batch isler.
    /// </summary>
    /// <param name="stoppingToken">Servis durdurma bildirimi.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxRelayWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "OutboxRelayWorker error");
            }

            await Task.Delay(TimeSpan.FromSeconds(DelaySeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var messages = await context.OutboxMessages
            .Where(message => !message.IsProcessed && message.RetryCount < 5)
            .OrderBy(message => message.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var topic = ResolveTopic(message.EventType);
                await publisher.PublishAsync(
                    topic,
                    message.EventType,
                    message.Payload,
                    stoppingToken);

                message.IsProcessed = true;
                message.ProcessedAt = DateTime.UtcNow;
                message.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                message.UpdatedAt = DateTime.UtcNow;

                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {Id}, retry {Count}",
                    message.Id,
                    message.RetryCount);
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }

    private static string ResolveTopic(string eventType)
    {
        return eventType switch
        {
            "booking.confirmed" => "biletix.notifications",
            "booking.payment_failed" => "biletix.notifications",
            "booking.cancelled" => "biletix.notifications",
            _ => "biletix.outbox"
        };
    }
}

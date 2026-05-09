using System.Text.Json;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Domain.Entities;
using Biletix.Infrastructure.Messaging.Models;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biletix.Infrastructure.Messaging.Consumers;

/// <summary>
/// Debezium tarafindan Kafka'ya yazilan Events CDC mesajlarini okuyup Elasticsearch indeksini senkronize eder.
/// </summary>
public sealed class EventCdcConsumer : BackgroundService
{
    private const string Topic = "biletix.public.Events";

    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventCdcConsumer> _logger;

    /// <summary>
    /// Kafka consumer ayarlarini hazirlar ve gerekli servisleri alir.
    /// </summary>
    /// <param name="configuration">Kafka ayarlarini tasiyan konfigurasyon.</param>
    /// <param name="scopeFactory">Scoped servisleri mesaj bazinda olusturmak icin scope factory.</param>
    /// <param name="logger">Consumer loglarini yazan logger.</param>
    public EventCdcConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<EventCdcConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "biletix-es-sync",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            MaxPollIntervalMs = 300000,
            SessionTimeoutMs = 30000
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Kafka topic'ini dinler ve basariyla islenen mesajlar icin manuel commit yapar.
    /// </summary>
    /// <param name="stoppingToken">Servis durdurma bildirimi.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);
        _logger.LogInformation("CDC consumer started, listening to {Topic}", Topic);
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                {
                    await Task.Delay(100, stoppingToken);
                    continue;
                }

                await ProcessMessageAsync(result.Message.Value, stoppingToken);
                _consumer.Commit(result);
            }
            catch (ConsumeException ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Kafka consume error");
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error processing CDC message");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _consumer.Close();
    }

    /// <summary>
    /// Tek bir Debezium mesajini isler ve Elasticsearch indeksini event'in guncel DB haline gore senkronize eder.
    /// </summary>
    /// <param name="messageValue">Kafka mesajinin JSON payload'u.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    private async Task ProcessMessageAsync(string messageValue, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(messageValue) || messageValue == "null")
        {
            return;
        }

        var message = JsonSerializer.Deserialize<DebeziumEventMessage>(messageValue);
        if (message is null)
        {
            return;
        }

        _logger.LogInformation(
            "CDC message received: op={Op}, table={Table}, id={Id}",
            message.Op,
            message.Table,
            message.Id);

        if (!Guid.TryParse(message.Id, out var eventId)) 
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var searchService = scope.ServiceProvider.GetRequiredService<IEventSearchService>();

        var isDeleted = message.Op == "d"
            || string.Equals(message.Deleted, "true", StringComparison.OrdinalIgnoreCase)
            || message.IsDeleted == true; // Debezium'un mesaj formatina ve kullandigi connector'a gore silme bilgisini farkli alanlarda tutabilir, bu nedenle birden fazla kontrol yapıyoruz.

        if (isDeleted)
        {
            await searchService.DeleteEventAsync(eventId, ct);
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var @event = await dbContext.Events
            .Include(item => item.Venue)
            .Include(item => item.Performer)
            .Include(item => item.TicketTypes)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == eventId, ct);

        if (@event is null)
        {
            return;
        }

        if (@event.Status != EventStatus.Published)
        {
            await searchService.DeleteEventAsync(eventId, ct);
            return;
        }

        await searchService.IndexEventAsync(ToSearchDocument(@event), ct);
        _logger.LogInformation("Event {Id} synced to Elasticsearch via CDC", eventId);
    }

    /// <summary>
    /// Event aggregate'ini Elasticsearch dokumanina map eder.
    /// </summary>
    /// <param name="event">Map edilecek etkinlik aggregate'i.</param>
    /// <returns>Elasticsearch etkinlik dokumani.</returns>
    private static EventSearchDocument ToSearchDocument(Event @event)
    {
        return new EventSearchDocument
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartDate = @event.StartDate,
            EndDate = @event.EndDate,
            Status = @event.Status.ToString(),
            ImageUrl = @event.ImageUrl,
            VenueId = @event.VenueId,
            VenueName = @event.Venue?.Name ?? string.Empty,
            VenueCity = @event.Venue?.City ?? string.Empty,
            VenueCapacity = @event.Venue?.Capacity ?? 0,
            PerformerId = @event.PerformerId,
            PerformerName = @event.Performer?.Name ?? string.Empty,
            PerformerGenre = @event.Performer?.Genre ?? string.Empty,
            MinPrice = @event.TicketTypes.Any() ? @event.TicketTypes.Min(ticketType => ticketType.Price) : 0,
            TotalAvailableTickets = @event.TicketTypes.Sum(ticketType => ticketType.AvailableCount),
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.UpdatedAt
        };
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}

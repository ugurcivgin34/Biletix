using System.Text;
using System.Text.Json;
using Biletix.Application.Common.Interfaces;
using Biletix.Infrastructure.Notifications.Models;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biletix.Infrastructure.Notifications;

/// <summary>
/// Kafka bildirim topic'ini dinleyip ilgili kullaniciya e-posta gonderen background consumer'dir.
/// </summary>
public sealed class NotificationKafkaConsumer : BackgroundService
{
    private const string Topic = "biletix.notifications";
    private const string GroupId = "biletix-notification-service";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationKafkaConsumer> _logger;

    /// <summary>
    /// Kafka consumer konfigurasyonunu olusturur.
    /// </summary>
    /// <param name="configuration">Kafka ayarlarini tasiyan konfigurasyon.</param>
    /// <param name="scopeFactory">Mesaj isleme sirasinda scoped servisleri olusturmak icin kullanilir.</param>
    /// <param name="logger">Consumer loglarini yazan logger.</param>
    public NotificationKafkaConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationKafkaConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            MaxPollIntervalMs = 300000,
            SessionTimeoutMs = 30000
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Kafka topic'ini dinler, basariyla islenen mesajlar icin manuel commit yapar.
    /// </summary>
    /// <param name="stoppingToken">Servis durdurma bildirimi.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);
        _logger.LogInformation("NotificationKafkaConsumer started, listening to {Topic}", Topic);
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

                var eventType = GetEventType(result.Message);
                await ProcessMessageAsync(eventType, result.Message.Value, stoppingToken);
                _consumer.Commit(result);
            }
            catch (ConsumeException ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "NotificationKafkaConsumer consume error");
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "NotificationKafkaConsumer message processing failed");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _consumer.Close();
    }

    // eventType'a gore ilgili mesaj isleme methodunu cagirir.
    private async Task ProcessMessageAsync(
        string eventType,
        string payload,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "NotificationKafkaConsumer: processing {EventType}",
            eventType);

        switch (eventType)
        {
            case "booking.confirmed":
                await ProcessBookingConfirmedAsync(payload, ct);
                break;
            case "booking.expired":
                await ProcessBookingExpiredAsync(payload, ct);
                break;
            case "booking.payment_failed":
                await ProcessPaymentFailedAsync(payload, ct);
                break;
            default:
                _logger.LogInformation("Unhandled notification event type: {EventType}", eventType);
                break;
        }
    }

    // bu methodlar, ilgili event icin gerekli verileri veritabanindan cekip email servisi ile kullaniciya bildirim gonderir.
    private async Task ProcessBookingConfirmedAsync(string payloadJson, CancellationToken ct)
    {
        var payload = Deserialize<BookingConfirmedPayload>(payloadJson);
        if (payload is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var qrTicketService = scope.ServiceProvider.GetRequiredService<IQrTicketService>();

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payload.UserId, ct);
        var @event = await context.Events
            .Include(item => item.Venue)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payload.EventId, ct);

        if (user is null || @event is null)
        {
            _logger.LogWarning(
                "Notification data missing for booking.confirmed. UserId={UserId}, EventId={EventId}",
                payload.UserId,
                payload.EventId);
            return;
        }

        var ticketToken = qrTicketService.GenerateTicketToken(
            payload.BookingId,
            payload.UserId,
            payload.EventId);
        var qrPngBytes = qrTicketService.GenerateQrCodePng(ticketToken);
        var qrContentId = $"ticket-qr-{payload.BookingId:N}@biletix";

        var html = EmailTemplates.BookingConfirmed(
            user.FirstName,
            @event.Title,
            @event.StartDate,
            @event.Venue?.Name ?? "TBD",
            payload.TotalAmount,
            payload.BookingId,
            $"cid:{qrContentId}");

        await emailService.SendAsync(
            user.Email,
            GetFullName(user.FirstName, user.LastName),
            $"Biletiniz Onaylandı - {@event.Title}",
            html,
            new[]
            {
                new EmailInlineAttachment(
                    qrContentId,
                    $"ticket-{payload.BookingId}.png",
                    "image/png",
                    qrPngBytes)
            },
            ct);
    }

    // booking.expired event'i icin benzer sekilde gerekli verileri cekip kullaniciya rezervasyonun suresinin doldugu bilgisini gonderir.
    private async Task ProcessBookingExpiredAsync(string payloadJson, CancellationToken ct) 
    {
        var payload = Deserialize<BookingExpiredPayload>(payloadJson);
        if (payload is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payload.UserId, ct);
        var booking = await context.Bookings
            .Include(item => item.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payload.BookingId, ct);

        if (user is null || booking?.Event is null)
        {
            _logger.LogWarning(
                "Notification data missing for booking.expired. UserId={UserId}, BookingId={BookingId}",
                payload.UserId,
                payload.BookingId);
            return;
        }

        var html = EmailTemplates.BookingExpired(user.FirstName, booking.Event.Title);
        await emailService.SendAsync(
            user.Email,
            GetFullName(user.FirstName, user.LastName),
            "Rezervasyonunuzun Süresi Doldu",
            html,
            inlineAttachments: null,
            ct);
    }

    // booking.payment_failed event'i icin benzer sekilde gerekli verileri cekip kullaniciya odemenin basarisiz oldugu bilgisini gonderir.
    private async Task ProcessPaymentFailedAsync(string payloadJson, CancellationToken ct)
    {
        var payload = Deserialize<BookingPaymentFailedPayload>(payloadJson);
        if (payload is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payload.UserId, ct);
        var booking = await context.Bookings
            .Include(item => item.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payload.BookingId, ct);

        if (user is null || booking?.Event is null)
        {
            _logger.LogWarning(
                "Notification data missing for booking.payment_failed. UserId={UserId}, BookingId={BookingId}",
                payload.UserId,
                payload.BookingId);
            return;
        }

        var html = EmailTemplates.PaymentFailed(user.FirstName, booking.Event.Title);
        await emailService.SendAsync(
            user.Email,
            GetFullName(user.FirstName, user.LastName),
            "Ödeme Başarısız",
            html,
            inlineAttachments: null,
            ct);
    }

    // Kafka mesajlarinin payload'ini ilgili modele deserialize eder. Deserialize edilemeyen mesajlar icin warning log'u yazilir ve isleme devam edilir.
    private T? Deserialize<T>(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Notification payload could not be deserialized");
            return default;
        }
    }

    // Kafka mesajlarinin eventType bilgisini header'dan veya key'ten alir. Header'da eventType bulunmazsa key de eventType olarak kullanilir.
    private static string GetEventType(Message<string, string> message)
    {
        var header = message.Headers?.FirstOrDefault(item =>
            string.Equals(item.Key, "eventType", StringComparison.OrdinalIgnoreCase));

        if (header is not null)
        {
            return Encoding.UTF8.GetString(header.GetValueBytes());
        }

        return message.Key;
    }

    private static string GetFullName(string firstName, string lastName)
    {
        return $"{firstName} {lastName}".Trim();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}

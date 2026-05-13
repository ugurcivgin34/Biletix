using System.Text.Json;
using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Payments.Commands.ExpireBookingOnFailure;

/// <summary>
/// Stripe payment failure webhook'u sonrasi rezervasyonu iptal eden ve ayrilan biletleri serbest birakan handler'dir.
/// </summary>
public sealed class ExpireBookingOnPaymentFailureCommandHandler
    : ICommandHandler<ExpireBookingOnPaymentFailureCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly ITicketLockService _ticketLockService;
    private readonly ILogger<ExpireBookingOnPaymentFailureCommandHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, odeme, Redis kilit ve logger servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="paymentService">Odeme saglayicisi servisi.</param>
    /// <param name="ticketLockService">Bilet tipi kilit servisi.</param>
    /// <param name="logger">Handler loglarini yazan logger.</param>
    public ExpireBookingOnPaymentFailureCommandHandler(
        IApplicationDbContext context,
        IPaymentService paymentService,
        ITicketLockService ticketLockService,
        ILogger<ExpireBookingOnPaymentFailureCommandHandler> logger)
    {
        _context = context;
        _paymentService = paymentService;
        _ticketLockService = ticketLockService;
        _logger = logger;
    }

    /// <summary>
    /// Payment intent kimligine bagli rezervasyonu iptal eder, kapasiteyi geri birakir ve outbox mesaji yazar.
    /// </summary>
    /// <param name="request">Odeme basarisizligi komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(
        ExpireBookingOnPaymentFailureCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(item => item.Items)
            .ThenInclude(item => item.TicketType)
            .FirstOrDefaultAsync(
                item => item.PaymentIntentId == request.PaymentIntentId,
                cancellationToken);

        if (booking is null)
        {
            _logger.LogWarning(
                "Booking not found for failed PaymentIntentId: {Id}",
                request.PaymentIntentId);
            return;
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Expired)
        {
            _logger.LogInformation(
                "Booking {Id} already {Status}, skipping failed payment webhook",
                booking.Id,
                booking.Status);
            return;
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            _logger.LogWarning(
                "Booking {Id} already confirmed, failed payment webhook ignored",
                booking.Id);
            return;
        }

        if (!string.IsNullOrWhiteSpace(booking.PaymentIntentId))
        {
            try
            {
                await _paymentService.CancelPaymentIntentAsync(
                    booking.PaymentIntentId,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Payment intent {PaymentIntentId} could not be cancelled after failure webhook",
                    booking.PaymentIntentId);
            }
        }

        foreach (var item in booking.Items)
        {
            item.TicketType?.ReleaseReservation(item.Quantity);
            await _ticketLockService.ReleaseLockAsync(item.TicketTypeId, booking.UserId);
        }

        booking.Cancel();

        var bookingIdText = booking.Id.ToString();
        var outboxExists = await _context.OutboxMessages.AnyAsync(
            message => message.EventType == "booking.payment_failed" &&
                EF.Functions.Like(message.Payload, $"%{bookingIdText}%"), // BookingId'nin payload icinde gecip gecmedigini kontrol eder
            cancellationToken);

        if (!outboxExists)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = "booking.payment_failed",
                Payload = JsonSerializer.Serialize(new
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    EventId = booking.EventId,
                    booking.TotalAmount,
                    PaymentIntentId = booking.PaymentIntentId,
                    Items = booking.Items.Select(item => new
                    {
                        TicketTypeName = item.TicketType?.Name,
                        item.Quantity,
                        item.UnitPrice
                    })
                }),
                IsProcessed = false,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(outboxMessage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Booking {Id} cancelled after Stripe payment failure", booking.Id);
    }
}

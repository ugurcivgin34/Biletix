using System.Text.Json;
using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Payments.Commands.ConfirmBooking;

/// <summary>
/// Stripe webhook basarisi sonrasi rezervasyonu kesinlestiren komut handler'idir.
/// </summary>
public sealed class ConfirmBookingCommandHandler : ICommandHandler<ConfirmBookingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITicketLockService _ticketLockService;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, Redis kilit ve logger servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="ticketLockService">Bilet tipi kilit servisi.</param>
    /// <param name="logger">Handler loglarini yazan logger.</param>
    public ConfirmBookingCommandHandler(
        IApplicationDbContext context,
        ITicketLockService ticketLockService,
        ILogger<ConfirmBookingCommandHandler> logger)
    {
        _context = context;
        _ticketLockService = ticketLockService;
        _logger = logger;
    }

    /// <summary>
    /// Payment intent kimligine bagli rezervasyonu bulur, satisa cevirir ve outbox mesaji yazar.
    /// </summary>
    /// <param name="request">Rezervasyon kesinlestirme komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
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
                "Booking not found for PaymentIntentId: {Id}",
                request.PaymentIntentId);
            return;
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            _logger.LogInformation("Booking {Id} already confirmed, skipping", booking.Id);
            return;
        }

        if (booking.Status != BookingStatus.Pending)
        {
            _logger.LogWarning(
                "Booking {Id} is in {Status} state and cannot be confirmed",
                booking.Id,
                booking.Status);
            return;
        }

        booking.Confirm(request.PaymentIntentId);

        foreach (var item in booking.Items)
        {
            var ticketType = item.TicketType
                ?? await _context.TicketTypes
                    .FirstOrDefaultAsync(type => type.Id == item.TicketTypeId, cancellationToken);

            ticketType?.ConfirmSale(item.Quantity);
        }

        foreach (var item in booking.Items)
        {
            await _ticketLockService.ReleaseLockAsync(item.TicketTypeId, booking.UserId);
        }

        var bookingIdText = booking.Id.ToString();
        var outboxExists = await _context.OutboxMessages.AnyAsync(
            message => message.EventType == "booking.confirmed" &&
                EF.Functions.Like(message.Payload, $"%{bookingIdText}%"),
            cancellationToken);

        if (!outboxExists)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = "booking.confirmed",
                Payload = JsonSerializer.Serialize(new
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    EventId = booking.EventId,
                    booking.TotalAmount,
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

        _logger.LogInformation("Booking {Id} confirmed via Stripe webhook", booking.Id);
    }
}

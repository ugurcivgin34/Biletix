using System.Text.Json;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Observability;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Bookings.Commands.ExpireBookings;

/// <summary>
/// Gecerlilik suresi dolmus pending rezervasyonlari expire eden komut handler'idir.
/// </summary>
public sealed class ExpireBookingsCommandHandler : ICommandHandler<ExpireBookingsCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ITicketLockService _ticketLockService;
    private readonly ILogger<ExpireBookingsCommandHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, Redis kilit ve logger servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="ticketLockService">Bilet tipi kilit servisi.</param>
    /// <param name="logger">Handler loglarini yazan logger.</param>
    public ExpireBookingsCommandHandler(
        IApplicationDbContext context,
        ITicketLockService ticketLockService,
        ILogger<ExpireBookingsCommandHandler> logger)
    {
        _context = context;
        _ticketLockService = ticketLockService;
        _logger = logger;
    }

    /// <summary>
    /// Pending ve ExpiresAt degeri gecmis rezervasyonlari expire eder, kapasiteyi geri birakir ve outbox mesaji yazar.
    /// </summary>
    /// <param name="request">Expire komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Expire edilen rezervasyon sayisi.</returns>
    public async Task<int> Handle(
        ExpireBookingsCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var expiredBookings = await _context.Bookings
            .Include(booking => booking.Items)
            .Where(booking => booking.Status == BookingStatus.Pending &&
                booking.ExpiresAt < utcNow)
            .ToListAsync(cancellationToken); // Expire edilmesi gereken rezervasyonlari veritabanindan cekiyoruz.

        foreach (var booking in expiredBookings)
        {
            booking.Expire();
            BiletixMetrics.BookingsExpired.Add(1);

            foreach (var item in booking.Items)
            {
                var ticketType = await _context.TicketTypes
                    .FirstOrDefaultAsync(
                        type => type.Id == item.TicketTypeId,
                        cancellationToken);

                ticketType?.ReleaseReservation(item.Quantity);
            }

            foreach (var item in booking.Items)
            {
                await _ticketLockService.ReleaseLockAsync(item.TicketTypeId, booking.UserId);
            }

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = "booking.expired",
                Payload = JsonSerializer.Serialize(new
                {
                    BookingId = booking.Id,
                    UserId = booking.UserId,
                    ExpiredAt = utcNow
                }),
                IsProcessed = false,
                RetryCount = 0,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expired {Count} bookings", expiredBookings.Count);

        return expiredBookings.Count;
    }
}

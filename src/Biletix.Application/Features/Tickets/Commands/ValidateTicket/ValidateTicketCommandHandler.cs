using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Tickets.Commands.ValidateTicket;

/// <summary>
/// QR bilet token'ini dogrular, scan kaydi yazar ve kapi gorevlisine sonuc dondurur.
/// </summary>
public sealed class ValidateTicketCommandHandler
    : ICommandHandler<ValidateTicketCommand, ValidateTicketResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrTicketService _qrTicketService;
    private readonly ILogger<ValidateTicketCommandHandler> _logger;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, QR servis ve logger bagimliliklarini alir.
    /// </summary>
    public ValidateTicketCommandHandler(
        IApplicationDbContext context,
        IQrTicketService qrTicketService,
        ILogger<ValidateTicketCommandHandler> logger)
    {
        _context = context;
        _qrTicketService = qrTicketService;
        _logger = logger;
    }

    /// <summary>
    /// QR token'i dogrular, tekrar kullanim ve booking durumu kontrollerini yapar.
    /// </summary>
    public async Task<ValidateTicketResponse> Handle(
        ValidateTicketCommand request,
        CancellationToken cancellationToken)
    {
        var scannedBy = string.IsNullOrWhiteSpace(request.ScannedBy)
            ? "unknown"
            : request.ScannedBy.Trim();

        var claims = _qrTicketService.ValidateTicketToken(request.QrToken);
        if (claims is null)
        {
            await SaveScanAsync(
                null,
                null,
                null,
                false,
                "Invalid or expired token",
                scannedBy,
                cancellationToken);

            return new ValidateTicketResponse(
                false,
                "❌ Geçersiz bilet",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                false,
                null);
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == claims.BookingId, cancellationToken);

        if (booking is null)
        {
            await SaveScanAsync(
                claims.BookingId,
                claims.EventId,
                claims.UserId,
                false,
                "Booking not found",
                scannedBy,
                cancellationToken);

            return new ValidateTicketResponse(
                false,
                "❌ Rezervasyon bulunamadı",
                claims.BookingId,
                claims.EventId,
                claims.UserId,
                null,
                null,
                null,
                null,
                false,
                null);
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            await SaveScanAsync(
                claims.BookingId,
                claims.EventId,
                claims.UserId,
                false,
                $"Booking status: {booking.Status}",
                scannedBy,
                cancellationToken);

            return new ValidateTicketResponse(
                false,
                $"❌ Bilet geçerli değil. Durum: {booking.Status}",
                claims.BookingId,
                claims.EventId,
                claims.UserId,
                null,
                null,
                null,
                null,
                false,
                null);
        }

        var previousScan = await _context.TicketScans
            .AsNoTracking()
            .Where(scan => scan.BookingId == claims.BookingId && scan.IsValid)
            .OrderBy(scan => scan.ScannedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (previousScan is not null)
        {
            await SaveScanAsync(
                claims.BookingId,
                claims.EventId,
                claims.UserId,
                false,
                "Already scanned",
                scannedBy,
                cancellationToken);

            return new ValidateTicketResponse(
                false,
                $"⚠️ Bu bilet daha önce kullanıldı ({previousScan.ScannedAt:HH:mm})",
                claims.BookingId,
                claims.EventId,
                claims.UserId,
                null,
                null,
                null,
                null,
                true,
                previousScan.ScannedAt);
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == claims.UserId, cancellationToken);
        var @event = await _context.Events
            .Include(item => item.Venue)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == claims.EventId, cancellationToken);

        await SaveScanAsync(
            claims.BookingId,
            claims.EventId,
            claims.UserId,
            true,
            null,
            scannedBy,
            cancellationToken);

        _logger.LogInformation(
            "Ticket scan validated. BookingId={BookingId}, EventId={EventId}, ScannedBy={ScannedBy}",
            claims.BookingId,
            claims.EventId,
            scannedBy);

        return new ValidateTicketResponse(
            true,
            "✅ Giriş onaylandı! Hoş geldiniz!",
            claims.BookingId,
            claims.EventId,
            claims.UserId,
            user?.FirstName,
            user?.LastName,
            @event?.Title,
            @event?.StartDate,
            false,
            null);
    }

    private async Task SaveScanAsync(
        Guid? bookingId,
        Guid? eventId,
        Guid? userId,
        bool isValid,
        string? invalidReason,
        string scannedBy,
        CancellationToken ct)
    {
        _context.TicketScans.Add(new TicketScan
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId ?? Guid.Empty,
            EventId = eventId ?? Guid.Empty,
            UserId = userId ?? Guid.Empty,
            ScannedAt = DateTime.UtcNow,
            ScannedBy = scannedBy,
            IsValid = isValid,
            InvalidReason = invalidReason
        });

        await _context.SaveChangesAsync(ct);
    }
}

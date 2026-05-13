using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Payments.Commands.CancelPayment;

/// <summary>
/// Rezervasyon iptali sirasinda Stripe payment intent'i iptal eden ve ayrilan biletleri serbest birakan handler'dir.
/// </summary>
public sealed class CancelPaymentCommandHandler : ICommandHandler<CancelPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;
    private readonly ITicketLockService _ticketLockService;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, kullanici, odeme ve kilit servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    /// <param name="paymentService">Odeme saglayicisi servisi.</param>
    /// <param name="ticketLockService">Bilet tipi kilit servisi.</param>
    public CancelPaymentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        ITicketLockService ticketLockService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
        _ticketLockService = ticketLockService;
    }

    /// <summary>
    /// Rezervasyon sahipligini dogrular, payment intent'i iptal eder ve ayrilan kapasiteyi geri birakir.
    /// </summary>
    /// <param name="request">Odeme iptal komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var booking = await _context.Bookings
            .Include(booking => booking.Items)
            .ThenInclude(item => item.TicketType)
            .FirstOrDefaultAsync(booking => booking.Id == request.BookingId, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException("Booking", request.BookingId);
        }

        if (booking.UserId != userId)
        {
            throw new DomainException("Access denied");
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            throw new DomainException("Cannot cancel confirmed booking");
        }

        if (!string.IsNullOrWhiteSpace(booking.PaymentIntentId))
        {
            await _paymentService.CancelPaymentIntentAsync(booking.PaymentIntentId, cancellationToken);
        }

        foreach (var item in booking.Items)
        {
            item.TicketType?.ReleaseReservation(item.Quantity);
            await _ticketLockService.ReleaseLockAsync(item.TicketTypeId, userId);
        }

        booking.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

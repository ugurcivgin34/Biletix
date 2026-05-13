using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;

/// <summary>
/// Pending rezervasyon icin Stripe payment intent olusturan komut handler'idir.
/// </summary>
public sealed class CreatePaymentIntentCommandHandler
    : ICommandHandler<CreatePaymentIntentCommand, CreatePaymentIntentResponse>
{
    private const string Currency = "try";

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentService _paymentService;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, kullanici ve odeme servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    /// <param name="paymentService">Odeme saglayicisi servisi.</param>
    public CreatePaymentIntentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentService paymentService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Rezervasyonu dogrular, Stripe payment intent olusturur ve payment intent id'sini rezervasyona kaydeder.
    /// </summary>
    /// <param name="request">Payment intent olusturma komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Frontend'in odeme baslatmasi icin gereken bilgiler.</returns>
    public async Task<CreatePaymentIntentResponse> Handle(
        CreatePaymentIntentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var booking = await _context.Bookings
            .Include(booking => booking.Items)
            .FirstOrDefaultAsync(booking => booking.Id == request.BookingId, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException("Booking", request.BookingId);
        }

        if (booking.UserId != userId)
        {
            throw new DomainException("Access denied");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new DomainException("Booking is not in pending state");
        }

        if (booking.IsExpired())
        {
            booking.Expire();
            await _context.SaveChangesAsync(cancellationToken);
            throw new DomainException("Booking has expired. Please make a new reservation.");
        }

        var idempotencyKey = $"payment-{booking.Id}";
        var result = await _paymentService.CreatePaymentIntentAsync(
            booking.Id,
            booking.TotalAmount,
            Currency,
            idempotencyKey,
            cancellationToken);

        booking.SetPaymentIntent(result.PaymentIntentId);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePaymentIntentResponse(
            booking.Id,
            result.ClientSecret,
            result.PaymentIntentId,
            booking.TotalAmount,
            booking.ExpiresAt);
    }
}

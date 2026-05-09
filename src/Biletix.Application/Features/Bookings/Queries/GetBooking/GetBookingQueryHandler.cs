using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Bookings.Queries.GetBooking;

/// <summary>
/// Rezervasyon detay sorgusunu sahiplik kontroluyle isler.
/// </summary>
public sealed class GetBookingQueryHandler : IQueryHandler<GetBookingQuery, BookingResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    public GetBookingQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Rezervasyonu item, ticket type ve event bilgileriyle okur.
    /// </summary>
    /// <param name="request">Rezervasyon detay sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Rezervasyon cevap modeli.</returns>
    public async Task<BookingResponse> Handle(GetBookingQuery request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(item => item.Event)
            .Include(item => item.Items)
            .ThenInclude(item => item.TicketType)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException("Booking", request.Id);
        }

        if (booking.UserId != _currentUserService.UserId && !_currentUserService.IsInRole("Admin"))
        {
            throw new DomainException("Access denied");
        }

        return booking.ToResponse(booking.Event?.Title ?? string.Empty);
    }
}

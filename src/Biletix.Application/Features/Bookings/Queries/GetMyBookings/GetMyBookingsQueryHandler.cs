using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Bookings.Queries.GetMyBookings;

/// <summary>
/// Aktif kullanicinin rezervasyonlarini sayfalanmis olarak donduren sorguyu isler.
/// </summary>
public sealed class GetMyBookingsQueryHandler : IQueryHandler<GetMyBookingsQuery, PagedResult<BookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    public GetMyBookingsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Aktif kullaniciya ait rezervasyonlari son olusturulandan eskiye dogru listeler.
    /// </summary>
    /// <param name="request">Kendi rezervasyonlarim sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis rezervasyon cevaplari.</returns>
    public async Task<PagedResult<BookingResponse>> Handle(
        GetMyBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Bookings
            .AsNoTracking()
            .Include(item => item.Event)
            .Include(item => item.Items)
            .ThenInclude(item => item.TicketType)
            .Where(item => item.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = bookings
            .Select(item => item.ToResponse(item.Event?.Title ?? string.Empty))
            .ToList();

        return new PagedResult<BookingResponse>(items, totalCount, page, pageSize);
    }
}

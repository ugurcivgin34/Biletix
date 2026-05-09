using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Events.DTOs;
using Biletix.Application.Features.Events.Mappers;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Queries.GetMyEvents;

/// <summary>
/// Aktif kullaniciya ait etkinlikleri listeleyen sorguyu isler.
/// </summary>
public sealed class GetMyEventsQueryHandler : IQueryHandler<GetMyEventsQuery, PagedResult<EventSummaryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici bilgisi servisi.</param>
    public GetMyEventsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Aktif kullanicinin olusturdugu tum durumdaki etkinlikleri sayfalanmis olarak dondurur.
    /// </summary>
    /// <param name="request">Kendi etkinliklerim sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis etkinlik ozet listesi.</returns>
    public async Task<PagedResult<EventSummaryResponse>> Handle(
        GetMyEventsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Events
            .AsNoTracking()
            .Include(item => item.Venue)
            .Include(item => item.Performer)
            .Include(item => item.TicketTypes)
            .Where(item => item.CreatedBy == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderBy(item => item.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = events
            .Select(item => item.ToSummaryResponse())
            .ToList();

        return new PagedResult<EventSummaryResponse>(items, totalCount, page, pageSize);
    }
}

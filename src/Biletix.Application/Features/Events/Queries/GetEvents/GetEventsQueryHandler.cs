using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Events.DTOs;
using Biletix.Application.Features.Events.Mappers;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Queries.GetEvents;

/// <summary>
/// Etkinlik listeleme sorgusunu filtreleme, yetki ve sayfalama kurallariyla isler.
/// </summary>
public sealed class GetEventsQueryHandler : IQueryHandler<GetEventsQuery, PagedResult<EventSummaryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici bilgisi servisi.</param>
    public GetEventsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Etkinlikleri filtreler, anonymous kullanicilar icin sadece yayindaki kayitlari dondurur.
    /// </summary>
    /// <param name="request">Etkinlik listeleme sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis etkinlik ozet listesi.</returns>
    public async Task<PagedResult<EventSummaryResponse>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Events
            .AsNoTracking()
            .Include(item => item.Venue)
            .Include(item => item.Performer)
            .Include(item => item.TicketTypes)
            .AsQueryable();

        query = ApplyFilters(query, request);

        if (!CanSeeAllStatuses())
        {
            query = query.Where(item => item.Status == EventStatus.Published);
        }

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

    private IQueryable<Event> ApplyFilters(IQueryable<Event> query, GetEventsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var pattern = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.Title, pattern));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(item => item.Venue != null && EF.Functions.ILike(item.Venue.City, request.City.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<EventStatus>(request.Status, true, out var status))
            {
                throw new DomainException("Invalid event status");
            }

            query = query.Where(item => item.Status == status);
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(item => item.StartDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(item => item.StartDate <= request.StartDateTo.Value);
        }

        if (request.PerformerId.HasValue)
        {
            query = query.Where(item => item.PerformerId == request.PerformerId.Value);
        }

        return query;
    }

    private bool CanSeeAllStatuses()
    {
        return _currentUserService.IsInRole("Organizer") || _currentUserService.IsInRole("Admin");
    }
}

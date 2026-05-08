using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Venues.DTOs;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Venues.Queries.GetVenues;

/// <summary>
/// Mekan listeleme sorgusunu filtreleme, siralama ve sayfalama kurallariyla isler.
/// </summary>
public sealed class GetVenuesQueryHandler : IQueryHandler<GetVenuesQuery, PagedResult<VenueResponse>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public GetVenuesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mekanlari opsiyonel filtrelerle arar, ada gore siralar ve sayfalanmis sonuc dondurur.
    /// </summary>
    /// <param name="request">Mekan listeleme sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis mekan listesi.</returns>
    public async Task<PagedResult<VenueResponse>> Handle(GetVenuesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Venues
            .AsNoTracking()
            .AsQueryable();

        query = ApplySearchFilter(query, request.SearchTerm);
        query = ApplyCityFilter(query, request.City);

        var totalCount = await query.CountAsync(cancellationToken);

        var venues = await query
            .OrderBy(venue => venue.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(venue => venue.ToResponse())
            .ToListAsync(cancellationToken);

        return new PagedResult<VenueResponse>(venues, totalCount, page, pageSize);
    }

    private static IQueryable<Venue> ApplySearchFilter(IQueryable<Venue> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var pattern = $"%{searchTerm.Trim()}%";
        return query.Where(venue => EF.Functions.ILike(venue.Name, pattern));
    }

    private static IQueryable<Venue> ApplyCityFilter(IQueryable<Venue> query, string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return query;
        }

        return query.Where(venue => EF.Functions.ILike(venue.City, city.Trim()));
    }
}

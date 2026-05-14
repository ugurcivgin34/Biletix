using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Performers.DTOs;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Performers.Queries.GetPerformers;

/// <summary>
/// Performer listeleme sorgusunu filtreleme ve sayfalama kurallariyla isler.
/// </summary>
public sealed class GetPerformersQueryHandler : IQueryHandler<GetPerformersQuery, PagedResult<PerformerResponse>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public GetPerformersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Performer kayitlarini opsiyonel arama filtresi ile sayfali dondurur.
    /// </summary>
    /// <param name="request">Performer listeleme sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis performer listesi.</returns>
    public async Task<PagedResult<PerformerResponse>> Handle(
        GetPerformersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Performers
            .AsNoTracking()
            .AsQueryable();

        query = ApplySearchFilter(query, request.SearchTerm);

        var totalCount = await query.CountAsync(cancellationToken);
        var performers = await query
            .OrderBy(performer => performer.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(performer => new PerformerResponse(
                performer.Id,
                performer.Name,
                performer.Genre,
                performer.ImageUrl))
            .ToListAsync(cancellationToken);

        return new PagedResult<PerformerResponse>(performers, totalCount, page, pageSize);
    }

    private static IQueryable<Performer> ApplySearchFilter(IQueryable<Performer> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var pattern = $"%{searchTerm.Trim()}%";
        return query.Where(performer => EF.Functions.ILike(performer.Name, pattern));
    }
}

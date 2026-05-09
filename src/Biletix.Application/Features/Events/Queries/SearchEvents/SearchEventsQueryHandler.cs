using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;

namespace Biletix.Application.Features.Events.Queries.SearchEvents;

/// <summary>
/// Elasticsearch etkinlik arama sorgusunu isler.
/// </summary>
public sealed class SearchEventsQueryHandler : IQueryHandler<SearchEventsQuery, PagedResult<EventSearchDocument>>
{
    private readonly IEventSearchService _eventSearchService;

    /// <summary>
    /// Handler'in ihtiyac duydugu arama servisini alir.
    /// </summary>
    /// <param name="eventSearchService">Etkinlik arama servisi.</param>
    public SearchEventsQueryHandler(IEventSearchService eventSearchService)
    {
        _eventSearchService = eventSearchService;
    }

    /// <summary>
    /// Query modelini search request modeline map eder ve Elasticsearch aramasini calistirir.
    /// </summary>
    /// <param name="request">Etkinlik arama sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Sayfalanmis arama sonucu.</returns>
    public Task<PagedResult<EventSearchDocument>> Handle(
        SearchEventsQuery request,
        CancellationToken cancellationToken)
    {
        var searchRequest = new EventSearchRequest
        {
            SearchTerm = request.SearchTerm,
            City = request.City,
            Genre = request.Genre,
            StartDateFrom = request.StartDateFrom,
            StartDateTo = request.StartDateTo,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Status = request.Status,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return _eventSearchService.SearchAsync(searchRequest, cancellationToken);
    }
}

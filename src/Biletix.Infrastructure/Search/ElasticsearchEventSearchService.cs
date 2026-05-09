using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Biletix.Infrastructure.Search;

/// <summary>
/// Etkinlik arama operasyonlarini Elasticsearch uzerinden gerceklestirir.
/// </summary>
public sealed class ElasticsearchEventSearchService : IEventSearchService
{
    private const string IndexName = "biletix-events";
    private readonly ElasticsearchClient _client;

    /// <summary>
    /// Elasticsearch client bagimliligini alir.
    /// </summary>
    /// <param name="client">Elasticsearch istemcisi.</param>
    public ElasticsearchEventSearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task IndexEventAsync(EventSearchDocument document, CancellationToken ct = default)
    {
        var response = await _client.IndexAsync(
            document,
            descriptor => descriptor.Index(IndexName).Id(document.Id.ToString()),
            ct);

        ThrowIfInvalid(response.IsValidResponse, response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task UpdateEventAsync(EventSearchDocument document, CancellationToken ct = default)
    {
        var response = await _client.UpdateAsync<EventSearchDocument, EventSearchDocument>(
            IndexName,
            document.Id.ToString(),
            descriptor => descriptor.Doc(document),
            ct);

        ThrowIfInvalid(response.IsValidResponse, response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task DeleteEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync(
            IndexName,
            eventId.ToString(),
            cancellationToken: ct);

        ThrowIfInvalid(response.IsValidResponse, response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task<PagedResult<EventSearchDocument>> SearchAsync(
        EventSearchRequest request,
        CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
        var mustClauses = BuildMustClauses(request);
        var filterClauses = BuildFilterClauses(request);
        var sortOptions = BuildSortOptions(request);

        var searchRequest = new SearchRequest<EventSearchDocument>(IndexName)
        {
            From = (page - 1) * pageSize,
            Size = pageSize,
            Query = new BoolQuery
            {
                Must = mustClauses,
                Filter = filterClauses
            },
            Sort = sortOptions
        };

        var response = await _client.SearchAsync<EventSearchDocument>(searchRequest, ct);
        ThrowIfInvalid(response.IsValidResponse, response.DebugInformation);

        var items = response.Hits
            .Select(hit => hit.Source!)
            .Where(source => source is not null)
            .ToList();

        var total = response.Total;

        return new PagedResult<EventSearchDocument>(items, (int)total, page, pageSize);
    }

    // Arama terimine gore aranacak alanlarda esnek arama yapmak icin kullanilacak sorgu kosullarini olusturur.
    private static List<Query> BuildMustClauses(EventSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return new List<Query> { new MatchAllQuery() };
        }

        return new List<Query>
        {
            new MultiMatchQuery
            {
                Fields = Fields.FromStrings(new[] { "title", "description", "performerName", "venueName" }),
                Query = request.SearchTerm,
                Fuzziness = new Fuzziness("AUTO"),
                Operator = Operator.Or
            }
        };
    }

    // Arama kriterlerine gore filtreleme yapmak icin kullanilacak sorgu kosullarini olusturur.
    private static List<Query> BuildFilterClauses(EventSearchRequest request)
    {
        var filterClauses = new List<Query>();

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            filterClauses.Add(new TermQuery
            {
                Field = "venueCity",
                Value = request.City
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            filterClauses.Add(new TermQuery
            {
                Field = "performerGenre",
                Value = request.Genre
            });
        }

        filterClauses.Add(new TermQuery
        {
            Field = "status",
            Value = !string.IsNullOrWhiteSpace(request.Status) ? request.Status : "Published"
        });

        if (request.StartDateFrom.HasValue || request.StartDateTo.HasValue)
        {
            filterClauses.Add(new DateRangeQuery
            {
                Field = "startDate",
                Gte = request.StartDateFrom,
                Lte = request.StartDateTo
            });
        }

        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            filterClauses.Add(new NumberRangeQuery
            {
                Field = "minPrice",
                Gte = (double?)request.MinPrice,
                Lte = (double?)request.MaxPrice
            });
        }

        return filterClauses;
    }

    // Arama sonucunu siralamak icin kullanilacak sort opsiyonlarini olusturur.
    private static List<SortOptions> BuildSortOptions(EventSearchRequest request)
    {
        var direction = request.SortDescending ? SortOrder.Desc : SortOrder.Asc;

        return request.SortBy.ToLowerInvariant() switch
        {
            "price" => new List<SortOptions>
            {
                new SortOptions { Field = new FieldSort("minPrice") { Order = direction } }
            },
            "relevance" => new List<SortOptions>
            {
                new SortOptions { Score = new ScoreSort { Order = SortOrder.Desc } }
            },
            _ => new List<SortOptions>
            {
                new SortOptions { Field = new FieldSort("startDate") { Order = direction } }
            }
        };
    }

    // Elasticsearch isteklerinin basarisiz olmasi durumunda istisna firlatir.
    private static void ThrowIfInvalid(bool isValid, string debugInformation)
    {
        if (!isValid)
        {
            throw new InvalidOperationException($"Elasticsearch operation failed: {debugInformation}");
        }
    }
}

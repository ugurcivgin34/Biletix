using Biletix.API.Common;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Events.Queries.SearchEvents;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;

namespace Biletix.API.Features.Search;

/// <summary>
/// Arama endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class SearchEndpoints : IEndpoint
{
    /// <summary>
    /// Etkinlik arama route'unu tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/search")
            .WithTags("Search");

        group.MapGet("/events", SearchEventsAsync)
            .AllowAnonymous()
            .RequireRateLimiting("search")
            .WithName("SearchEvents")
            .Produces<PagedResult<EventSearchDocument>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> SearchEventsAsync(
        ISender sender,
        string? q = null,
        string? city = null,
        string? genre = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null,
        string sortBy = "startDate",
        bool sortDesc = false,
        int page = 1,
        int pageSize = 20)
    {
        var response = await sender.Send(new SearchEventsQuery
        {
            SearchTerm = q,
            City = city,
            Genre = genre,
            StartDateFrom = startDateFrom,
            StartDateTo = startDateTo,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Status = status,
            SortBy = sortBy,
            SortDescending = sortDesc,
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }
}

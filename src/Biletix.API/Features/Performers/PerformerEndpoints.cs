using Biletix.API.Common;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Performers.Commands.CreatePerformer;
using Biletix.Application.Features.Performers.DTOs;
using Biletix.Application.Features.Performers.Queries.GetPerformers;
using MediatR;

namespace Biletix.API.Features.Performers;

/// <summary>
/// Performer listeleme ve olusturma endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class PerformerEndpoints : IEndpoint
{
    /// <summary>
    /// Performer route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/performers")
            .WithTags("Performers");

        group.MapGet(string.Empty, GetPerformersAsync)
            .AllowAnonymous()
            .WithName("GetPerformers")
            .Produces<PagedResult<PerformerResponse>>(StatusCodes.Status200OK);

        group.MapPost(string.Empty, CreatePerformerAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("CreatePerformer")
            .Produces<Guid>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetPerformersAsync(
        ISender sender,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20)
    {
        var response = await sender.Send(new GetPerformersQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> CreatePerformerAsync(CreatePerformerRequest request, ISender sender)
    {
        var performerId = await sender.Send(new CreatePerformerCommand
        {
            Name = request.Name,
            Genre = request.Genre,
            ImageUrl = request.ImageUrl
        });

        return Results.Created($"/api/performers/{performerId}", performerId);
    }
}

/// <summary>
/// Performer olusturma endpoint'i icin request modelidir.
/// </summary>
/// <param name="Name">Performer gorunen adi.</param>
/// <param name="Genre">Performer turu veya janri.</param>
/// <param name="ImageUrl">Opsiyonel performer gorsel adresi.</param>
public sealed record CreatePerformerRequest(string Name, string Genre, string? ImageUrl);

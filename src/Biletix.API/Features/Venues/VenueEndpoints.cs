using Biletix.API.Common;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Venues.Commands.CreateVenue;
using Biletix.Application.Features.Venues.Commands.DeleteVenue;
using Biletix.Application.Features.Venues.Commands.UpdateVenue;
using Biletix.Application.Features.Venues.DTOs;
using Biletix.Application.Features.Venues.Queries.GetVenue;
using Biletix.Application.Features.Venues.Queries.GetVenues;
using MediatR;

namespace Biletix.API.Features.Venues;

/// <summary>
/// Mekan CRUD endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class VenueEndpoints : IEndpoint
{
    /// <summary>
    /// Mekan listeleme, detay, olusturma, guncelleme ve silme route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/venues")
            .WithTags("Venues");

        group.MapGet(string.Empty, GetVenuesAsync)
            .AllowAnonymous()
            .WithName("GetVenues")
            .Produces<PagedResult<VenueResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetVenueAsync)
            .AllowAnonymous()
            .WithName("GetVenue")
            .Produces<VenueResponse>(StatusCodes.Status200OK);

        group.MapPost(string.Empty, CreateVenueAsync)
            .RequireAuthorization("AdminOnly")
            .WithName("CreateVenue")
            .Produces<Guid>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", UpdateVenueAsync)
            .RequireAuthorization("AdminOnly")
            .WithName("UpdateVenue")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{id:guid}", DeleteVenueAsync)
            .RequireAuthorization("AdminOnly")
            .WithName("DeleteVenue")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetVenuesAsync(
        ISender sender,
        string? searchTerm = null,
        string? city = null,
        int page = 1,
        int pageSize = 20)
    {
        var response = await sender.Send(new GetVenuesQuery
        {
            SearchTerm = searchTerm,
            City = city,
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> GetVenueAsync(Guid id, ISender sender)
    {
        var response = await sender.Send(new GetVenueQuery { Id = id });
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateVenueAsync(CreateVenueRequest request, ISender sender)
    {
        var venueId = await sender.Send(new CreateVenueCommand
        {
            Name = request.Name,
            City = request.City,
            Address = request.Address,
            Capacity = request.Capacity
        });

        return Results.Created($"/api/venues/{venueId}", venueId);
    }

    private static async Task<IResult> UpdateVenueAsync(Guid id, UpdateVenueRequest request, ISender sender)
    {
        await sender.Send(new UpdateVenueCommand
        {
            Id = id,
            Name = request.Name,
            City = request.City,
            Address = request.Address,
            Capacity = request.Capacity
        });

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteVenueAsync(Guid id, ISender sender)
    {
        await sender.Send(new DeleteVenueCommand { Id = id });
        return Results.NoContent();
    }
}

/// <summary>
/// Mekan olusturma endpoint'i icin request modelidir.
/// </summary>
/// <param name="Name">Olusturulacak mekanin gorunen adi.</param>
/// <param name="City">Olusturulacak mekanin bulundugu sehir.</param>
/// <param name="Address">Olusturulacak mekanin acik adresi.</param>
/// <param name="Capacity">Olusturulacak mekanin toplam kapasitesi.</param>
public sealed record CreateVenueRequest(string Name, string City, string Address, int Capacity);

/// <summary>
/// Mekan guncelleme endpoint'i icin request modelidir.
/// </summary>
/// <param name="Name">Mekanin yeni gorunen adi.</param>
/// <param name="City">Mekanin yeni sehir bilgisi.</param>
/// <param name="Address">Mekanin yeni acik adresi.</param>
/// <param name="Capacity">Mekanin yeni toplam kapasitesi.</param>
public sealed record UpdateVenueRequest(string Name, string City, string Address, int Capacity);

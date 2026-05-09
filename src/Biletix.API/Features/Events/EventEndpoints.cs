using Biletix.API.Common;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Events.Commands.AddTicketType;
using Biletix.Application.Features.Events.Commands.CancelEvent;
using Biletix.Application.Features.Events.Commands.CreateEvent;
using Biletix.Application.Features.Events.Commands.PublishEvent;
using Biletix.Application.Features.Events.Commands.UpdateEvent;
using Biletix.Application.Features.Events.DTOs;
using Biletix.Application.Features.Events.Queries.GetEvent;
using Biletix.Application.Features.Events.Queries.GetEvents;
using Biletix.Application.Features.Events.Queries.GetMyEvents;
using MediatR;

namespace Biletix.API.Features.Events;

/// <summary>
/// Etkinlik CRUD ve yayin akis endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class EventEndpoints : IEndpoint
{
    /// <summary>
    /// Etkinlik listeleme, detay, olusturma, guncelleme, yayinlama ve bilet tipi route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events");

        group.MapGet(string.Empty, GetEventsAsync)
            .AllowAnonymous()
            .WithName("GetEvents")
            .Produces<PagedResult<EventSummaryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/my", GetMyEventsAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("GetMyEvents")
            .Produces<PagedResult<EventSummaryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetEventAsync)
            .AllowAnonymous()
            .WithName("GetEvent")
            .Produces<EventResponse>(StatusCodes.Status200OK);

        group.MapPost(string.Empty, CreateEventAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("CreateEvent")
            .Produces<Guid>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", UpdateEventAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("UpdateEvent")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/{id:guid}/publish", PublishEventAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("PublishEvent")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/cancel", CancelEventAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("CancelEvent")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/ticket-types", AddTicketTypeAsync)
            .RequireAuthorization("OrganizerOrAdmin")
            .WithName("AddTicketType")
            .Produces<Guid>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetEventsAsync(
        ISender sender,
        string? searchTerm = null,
        string? city = null,
        string? status = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        Guid? performerId = null,
        int page = 1,
        int pageSize = 20)
    {
        var response = await sender.Send(new GetEventsQuery
        {
            SearchTerm = searchTerm,
            City = city,
            Status = status,
            StartDateFrom = startDateFrom,
            StartDateTo = startDateTo,
            PerformerId = performerId,
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> GetEventAsync(Guid id, ISender sender)
    {
        var response = await sender.Send(new GetEventQuery { Id = id });
        return Results.Ok(response);
    }

    private static async Task<IResult> GetMyEventsAsync(ISender sender, int page = 1, int pageSize = 20)
    {
        var response = await sender.Send(new GetMyEventsQuery
        {
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateEventAsync(CreateEventRequest request, ISender sender)
    {
        var eventId = await sender.Send(new CreateEventCommand
        {
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            VenueId = request.VenueId,
            PerformerId = request.PerformerId,
            ImageUrl = request.ImageUrl,
            TicketTypes = (request.TicketTypes ?? new List<CreateTicketTypeRequest>())
                .Select(item => new CreateTicketTypeDto(item.Name, item.Price, item.TotalCapacity))
                .ToList()
        });

        return Results.Created($"/api/events/{eventId}", eventId);
    }

    private static async Task<IResult> UpdateEventAsync(Guid id, UpdateEventRequest request, ISender sender)
    {
        await sender.Send(new UpdateEventCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ImageUrl = request.ImageUrl
        });

        return Results.NoContent();
    }

    private static async Task<IResult> PublishEventAsync(Guid id, ISender sender)
    {
        await sender.Send(new PublishEventCommand { Id = id });
        return Results.Ok(new { message = "Event published successfully" });
    }

    private static async Task<IResult> CancelEventAsync(Guid id, CancelEventRequest request, ISender sender)
    {
        await sender.Send(new CancelEventCommand
        {
            Id = id,
            Reason = request.Reason
        });

        return Results.Ok(new { message = "Event cancelled" });
    }

    private static async Task<IResult> AddTicketTypeAsync(Guid id, AddTicketTypeRequest request, ISender sender)
    {
        var ticketTypeId = await sender.Send(new AddTicketTypeCommand
        {
            EventId = id,
            Name = request.Name,
            Price = request.Price,
            TotalCapacity = request.TotalCapacity
        });

        return Results.Created($"/api/events/{id}/ticket-types/{ticketTypeId}", ticketTypeId);
    }
}

/// <summary>
/// Etkinlik olusturma endpoint'i icin request modelidir.
/// </summary>
/// <param name="Title">Etkinligin gorunen basligi.</param>
/// <param name="Description">Etkinligin detay aciklamasi.</param>
/// <param name="StartDate">Etkinligin baslangic tarihi.</param>
/// <param name="EndDate">Etkinligin bitis tarihi.</param>
/// <param name="VenueId">Etkinligin yapilacagi mekan kimligi.</param>
/// <param name="PerformerId">Etkinlikte yer alacak performer kimligi.</param>
/// <param name="ImageUrl">Etkinlik icin opsiyonel gorsel adresi.</param>
/// <param name="TicketTypes">Etkinlik icin olusturulacak bilet tipleri.</param>
public sealed record CreateEventRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    Guid VenueId,
    Guid PerformerId,
    string? ImageUrl,
    List<CreateTicketTypeRequest> TicketTypes);

/// <summary>
/// Etkinlik olusturma sirasinda gelen bilet tipi request modelidir.
/// </summary>
/// <param name="Name">Bilet tipinin gorunen adi.</param>
/// <param name="Price">Bilet tipinin birim fiyati.</param>
/// <param name="TotalCapacity">Bilet tipi icin toplam kapasite.</param>
public sealed record CreateTicketTypeRequest(string Name, decimal Price, int TotalCapacity);

/// <summary>
/// Etkinlik guncelleme endpoint'i icin request modelidir.
/// </summary>
/// <param name="Title">Etkinligin yeni basligi.</param>
/// <param name="Description">Etkinligin yeni aciklamasi.</param>
/// <param name="StartDate">Etkinligin yeni baslangic tarihi.</param>
/// <param name="EndDate">Etkinligin yeni bitis tarihi.</param>
/// <param name="ImageUrl">Etkinligin yeni opsiyonel gorsel adresi.</param>
public sealed record UpdateEventRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string? ImageUrl);

/// <summary>
/// Etkinlik iptal endpoint'i icin request modelidir.
/// </summary>
/// <param name="Reason">Iptal gerekcesi.</param>
public sealed record CancelEventRequest(string Reason);

/// <summary>
/// Etkinlige bilet tipi ekleme endpoint'i icin request modelidir.
/// </summary>
/// <param name="Name">Bilet tipinin gorunen adi.</param>
/// <param name="Price">Bilet tipinin birim fiyati.</param>
/// <param name="TotalCapacity">Bilet tipi icin toplam kapasite.</param>
public sealed record AddTicketTypeRequest(string Name, decimal Price, int TotalCapacity);

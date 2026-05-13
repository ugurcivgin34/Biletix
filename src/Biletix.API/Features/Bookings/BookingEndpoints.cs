using Biletix.API.Common;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Bookings.Commands.ReserveTickets;
using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Application.Features.Bookings.Queries.GetBooking;
using Biletix.Application.Features.Bookings.Queries.GetMyBookings;
using Biletix.Application.Features.Bookings.Saga;
using Biletix.Application.Common.Interfaces;
using MediatR;

namespace Biletix.API.Features.Bookings;

/// <summary>
/// Rezervasyon endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class BookingEndpoints : IEndpoint
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    /// <summary>
    /// Rezervasyon olusturma, detay ve kullanicinin rezervasyonlari route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings")
            .RequireAuthorization("AuthenticatedUser");

        group.MapPost("/reserve", ReserveTicketsAsync)
            .WithName("ReserveTickets")
            .Produces<BookingResponse>(StatusCodes.Status201Created);

        group.MapPost("/checkout", CheckoutAsync)
            .WithName("Checkout")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/my", GetMyBookingsAsync)
            .WithName("GetMyBookings")
            .Produces<PagedResult<BookingResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetBookingAsync)
            .WithName("GetBooking")
            .Produces<BookingResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ReserveTicketsAsync(
        ReserveTicketsRequest request,
        HttpContext httpContext,
        ISender sender)
    {
        if (!httpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKeyValues)
            || string.IsNullOrWhiteSpace(idempotencyKeyValues.FirstOrDefault()))
        {
            return Results.BadRequest("Idempotency-Key header is required");
        }

        var response = await sender.Send(new ReserveTicketsCommand
        {
            EventId = request.EventId,
            IdempotencyKey = idempotencyKeyValues.First()!,
            Items = (request.Items ?? new List<ReserveTicketItemRequest>())
                .Select(item => new ReserveTicketItemDto(item.TicketTypeId, item.Quantity))
                .ToList()
        });

        return Results.Created($"/api/bookings/{response.Id}", response);
    }

    private static async Task<IResult> CheckoutAsync(
        CheckoutRequest request,
        HttpContext httpContext,
        ICurrentUserService currentUserService,
        IBookingSaga bookingSaga,
        CancellationToken ct)
    {
        var idempotencyKey = httpContext.Request.Headers[IdempotencyKeyHeader].ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Results.BadRequest("Idempotency-Key header is required");
        }

        if (currentUserService.UserId is not { } userId)
        {
            return Results.Unauthorized();
        }

        var result = await bookingSaga.ExecuteAsync(
            request.EventId,
            userId,
            (request.Items ?? new List<ReserveTicketItemRequest>())
                .Select(item => new ReserveTicketItemDto(item.TicketTypeId, item.Quantity))
                .ToList(),
            idempotencyKey,
            ct);

        if (!result.IsSuccess)
        {
            return Results.UnprocessableEntity(new
            {
                error = result.Error,
                state = result.FinalState.ToString()
            });
        }

        return Results.Ok(new
        {
            bookingId = result.BookingId,
            clientSecret = result.ClientSecret,
            message = "Booking created. Use clientSecret to complete payment."
        });
    }

    private static async Task<IResult> GetBookingAsync(Guid id, ISender sender)
    {
        var response = await sender.Send(new GetBookingQuery { Id = id });
        return Results.Ok(response);
    }

    private static async Task<IResult> GetMyBookingsAsync(ISender sender, int page = 1, int pageSize = 20)
    {
        var response = await sender.Send(new GetMyBookingsQuery
        {
            Page = page,
            PageSize = pageSize
        });

        return Results.Ok(response);
    }
}

/// <summary>
/// Bilet rezervasyon endpoint'i icin request modelidir.
/// </summary>
/// <param name="EventId">Rezervasyon yapilacak etkinlik kimligi.</param>
/// <param name="Items">Rezerve edilecek bilet kalemleri.</param>
public sealed record ReserveTicketsRequest(Guid EventId, List<ReserveTicketItemRequest> Items);

/// <summary>
/// Checkout endpoint'i icin request modelidir.
/// </summary>
/// <param name="EventId">Rezervasyon yapilacak etkinlik kimligi.</param>
/// <param name="Items">Rezerve edilecek bilet kalemleri.</param>
public sealed record CheckoutRequest(Guid EventId, List<ReserveTicketItemRequest> Items);

/// <summary>
/// Rezervasyon istegindeki tek bir bilet kalemi request modelidir.
/// </summary>
/// <param name="TicketTypeId">Rezerve edilecek bilet tipi kimligi.</param>
/// <param name="Quantity">Rezerve edilecek bilet adedi.</param>
public sealed record ReserveTicketItemRequest(Guid TicketTypeId, int Quantity);

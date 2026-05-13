using Biletix.API.Common;
using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biletix.API.Features.Tickets;

/// <summary>
/// QR bilet endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class TicketEndpoints : IEndpoint
{
    /// <summary>
    /// QR PNG ve QR token endpoint'lerini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .RequireAuthorization("AuthenticatedUser");

        group.MapGet("/{bookingId:guid}/qr", GetQrAsync)
            .WithName("GetTicketQr")
            .Produces(StatusCodes.Status200OK, contentType: "image/png")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{bookingId:guid}/token", GetTokenAsync)
            .WithName("GetTicketToken")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetQrAsync(
        Guid bookingId,
        IQrTicketService qrTicketService,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        var booking = await GetAuthorizedConfirmedBookingAsync(
            bookingId,
            context,
            currentUserService,
            ct);

        if (booking.Result is not null)
        {
            return booking.Result;
        }

        var ticket = booking.Booking!;
        var token = qrTicketService.GenerateTicketToken(
            ticket.Id,
            ticket.UserId,
            ticket.EventId);
        var pngBytes = qrTicketService.GenerateQrCodePng(token);

        return Results.File(pngBytes, "image/png", $"ticket-{bookingId}.png");
    }

    private static async Task<IResult> GetTokenAsync(
        Guid bookingId,
        IQrTicketService qrTicketService,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        var booking = await GetAuthorizedConfirmedBookingAsync(
            bookingId,
            context,
            currentUserService,
            ct);

        if (booking.Result is not null)
        {
            return booking.Result;
        }

        var ticket = booking.Booking!;
        var token = qrTicketService.GenerateTicketToken(
            ticket.Id,
            ticket.UserId,
            ticket.EventId);
        var claims = qrTicketService.ValidateTicketToken(token);

        return Results.Ok(new
        {
            token,
            bookingId = ticket.Id,
            eventId = ticket.EventId,
            expiresAt = claims?.ExpiresAt
        });
    }

    private static async Task<(Booking? Booking, IResult? Result)> GetAuthorizedConfirmedBookingAsync(
        Guid bookingId,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        var booking = await context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == bookingId, ct);

        if (booking is null)
        {
            return (null, Results.NotFound());
        }

        if (booking.UserId != currentUserService.UserId &&
            !currentUserService.IsInRole("Admin"))
        {
            return (null, Results.Forbid());
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return (null, Results.BadRequest(new
            {
                error = "Only confirmed bookings have QR tickets"
            }));
        }

        return (booking, null);
    }
}

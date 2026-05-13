using Biletix.API.Common;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Tickets.Commands.ValidateTicket;
using Biletix.Domain.Entities;
using MediatR;
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

        group.MapPost("/validate", ValidateTicketAsync)
            .WithName("ValidateTicket")
            .RequireAuthorization("OrganizerOrAdmin")
            .Produces<ValidateTicketResponse>(StatusCodes.Status200OK);

        group.MapGet("/scans/{eventId:guid}", GetScanHistoryAsync)
            .WithName("GetTicketScanHistory")
            .RequireAuthorization("OrganizerOrAdmin")
            .Produces(StatusCodes.Status200OK);
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

    private static async Task<IResult> ValidateTicketAsync(
        ValidateTicketRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var response = await sender.Send(new ValidateTicketCommand
        {
            QrToken = request.QrToken,
            ScannedBy = request.ScannedBy
        }, ct);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetScanHistoryAsync(
        Guid eventId,
        int page,
        int pageSize,
        IApplicationDbContext context,
        CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is <= 0 or > 200 ? 50 : pageSize;

        var scans = await context.TicketScans
            .AsNoTracking()
            .Where(scan => scan.EventId == eventId)
            .OrderByDescending(scan => scan.ScannedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupJoin(
                context.Users.AsNoTracking(),
                scan => scan.UserId,
                user => user.Id,
                (scan, users) => new
                {
                    Scan = scan,
                    User = users.FirstOrDefault()
                })
            .Select(item => new
            {
                bookingId = item.Scan.BookingId,
                scannedAt = item.Scan.ScannedAt,
                isValid = item.Scan.IsValid,
                invalidReason = item.Scan.InvalidReason,
                attendeeName = item.User == null
                    ? null
                    : (item.User.FirstName + " " + item.User.LastName).Trim(),
                scannedBy = item.Scan.ScannedBy
            })
            .ToListAsync(ct);

        return Results.Ok(scans);
    }
}

/// <summary>
/// QR bilet dogrulama endpoint'i request modelidir.
/// </summary>
/// <param name="QrToken">QR koddan okunan JWT imzali bilet token'i.</param>
/// <param name="ScannedBy">Taramayi yapan kapi gorevlisi, cihaz veya turnike kimligi.</param>
public sealed record ValidateTicketRequest(string QrToken, string ScannedBy);

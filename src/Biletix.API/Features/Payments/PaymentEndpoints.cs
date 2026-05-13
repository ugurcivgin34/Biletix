using Biletix.API.Common;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Payments.Commands.CancelPayment;
using Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;
using Biletix.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Biletix.API.Features.Payments;

/// <summary>
/// Odeme endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class PaymentEndpoints : IEndpoint
{
    /// <summary>
    /// Payment intent olusturma, iptal ve rezervasyon odeme durumu route'larini tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .RequireAuthorization("AuthenticatedUser");

        group.MapPost("/create-intent", CreatePaymentIntentAsync)
            .WithName("CreatePaymentIntent")
            .Produces<CreatePaymentIntentResponse>(StatusCodes.Status200OK);

        group.MapPost("/cancel", CancelPaymentAsync)
            .WithName("CancelPayment")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/booking/{bookingId:guid}", GetBookingPaymentStatusAsync)
            .WithName("GetBookingPaymentStatus")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        ISender sender)
    {
        var response = await sender.Send(new CreatePaymentIntentCommand
        {
            BookingId = request.BookingId
        });

        return Results.Ok(response);
    }

    private static async Task<IResult> CancelPaymentAsync(
        CancelPaymentRequest request,
        ISender sender)
    {
        await sender.Send(new CancelPaymentCommand
        {
            BookingId = request.BookingId
        });

        return Results.NoContent();
    }

    private static async Task<IResult> GetBookingPaymentStatusAsync(
        Guid bookingId,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var booking = await context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == bookingId, ct);

        if (booking is null)
        {
            throw new NotFoundException("Booking", bookingId);
        }

        if (booking.UserId != userId && !currentUserService.IsInRole("Admin"))
        {
            throw new DomainException("Access denied");
        }

        return Results.Ok(new
        {
            bookingId = booking.Id,
            status = booking.Status.ToString(),
            paymentIntentId = booking.PaymentIntentId,
            amount = booking.TotalAmount,
            expiresAt = booking.ExpiresAt
        });
    }
}

/// <summary>
/// Payment intent olusturma endpoint'i request modelidir.
/// </summary>
/// <param name="BookingId">Odeme niyeti olusturulacak rezervasyon kimligi.</param>
public sealed record CreatePaymentIntentRequest(Guid BookingId);

/// <summary>
/// Odeme iptal endpoint'i request modelidir.
/// </summary>
/// <param name="BookingId">Iptal edilecek rezervasyon kimligi.</param>
public sealed record CancelPaymentRequest(Guid BookingId);

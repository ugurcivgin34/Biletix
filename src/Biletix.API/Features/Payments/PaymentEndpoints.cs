using Biletix.API.Common;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Payments.Commands.CancelPayment;
using Biletix.Application.Features.Payments.Commands.ConfirmBooking;
using Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;
using Biletix.Domain.Entities;
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

        group.MapPost("/confirm-client", ConfirmClientPaymentAsync)
            .WithName("ConfirmClientPayment")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

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

    private static async Task<IResult> ConfirmClientPaymentAsync(
        ConfirmClientPaymentRequest request,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPaymentService paymentService,
        ISender sender,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var booking = await context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.BookingId, ct);

        if (booking is null)
        {
            throw new NotFoundException("Booking", request.BookingId);
        }

        if (booking.UserId != userId && !currentUserService.IsInRole("Admin"))
        {
            throw new DomainException("Access denied");
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            return Results.Ok(new
            {
                bookingId = booking.Id,
                status = booking.Status.ToString()
            });
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new DomainException("Booking is not in pending state");
        }

        if (booking.PaymentIntentId != request.PaymentIntentId)
        {
            throw new DomainException("Payment intent does not match booking");
        }

        var paymentIntent = await paymentService.GetPaymentIntentStatusAsync(
            request.PaymentIntentId,
            ct);

        if (paymentIntent.BookingId.HasValue && paymentIntent.BookingId.Value != booking.Id)
        {
            throw new DomainException("Payment intent metadata does not match booking");
        }

        if (!string.Equals(paymentIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException($"Payment is not completed. Current status: {paymentIntent.Status}");
        }

        await sender.Send(new ConfirmBookingCommand(request.PaymentIntentId), ct);

        return Results.Ok(new
        {
            bookingId = booking.Id,
            status = BookingStatus.Confirmed.ToString()
        });
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

/// <summary>
/// Client tarafinda basarili donen odemeyi Stripe uzerinden dogrulayip rezervasyonu onaylama request modelidir.
/// </summary>
/// <param name="BookingId">Onaylanacak rezervasyon kimligi.</param>
/// <param name="PaymentIntentId">Stripe payment intent kimligi.</param>
public sealed record ConfirmClientPaymentRequest(Guid BookingId, string PaymentIntentId);

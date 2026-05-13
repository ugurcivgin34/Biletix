using Biletix.Application.Features.Bookings.Commands.ReserveTickets;
using Biletix.Application.Features.Payments.Commands.CancelPayment;
using Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;
using Biletix.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Biletix.Application.Features.Bookings.Saga;

/// <summary>
/// Checkout akisini bilet rezervasyonu ve payment intent olusturma adimlariyla orkestre eder.
/// </summary>
public sealed class BookingSaga : IBookingSaga
{
    private readonly ISender _sender;
    private readonly ILogger<BookingSaga> _logger;

    /// <summary>
    /// Saga'nin ihtiyac duydugu mediator ve logger servislerini alir.
    /// </summary>
    /// <param name="sender">Command'leri calistiran MediatR sender.</param>
    /// <param name="logger">Saga loglarini yazan logger.</param>
    public BookingSaga(ISender sender, ILogger<BookingSaga> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<BookingSagaResult> ExecuteAsync(
        Guid eventId,
        Guid userId,
        List<ReserveTicketItemDto> items,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        var state = BookingSagaState.Started;
        Guid? bookingId = null;
        string? clientSecret = null;

        try
        {
            _logger.LogInformation("Saga: Reserving tickets for user {UserId}", userId);

            var reserveCommand = new ReserveTicketsCommand(eventId, items, idempotencyKey);
            var booking = await _sender.Send(reserveCommand, ct);
            bookingId = booking.Id;
            state = BookingSagaState.TicketsReserved;

            _logger.LogInformation("Saga: Creating payment intent for booking {Id}", bookingId);

            var paymentCommand = new CreatePaymentIntentCommand(bookingId.Value);
            var paymentResult = await _sender.Send(paymentCommand, ct);
            clientSecret = paymentResult.ClientSecret;
            state = BookingSagaState.PaymentIntentCreated;

            _logger.LogInformation(
                "Saga: Completed successfully. BookingId={Id}, State={State}",
                bookingId,
                state);

            return new BookingSagaResult(true, bookingId, clientSecret, null, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Saga failed at state {State} for user {UserId}",
                state,
                userId);

            state = BookingSagaState.Compensating;

            var compensated = await CompensateAsync(bookingId, state, ct);
            state = compensated ? BookingSagaState.Compensated : BookingSagaState.Failed;

            return new BookingSagaResult(false, bookingId, null, ex.Message, state);
        }
    }

    private async Task<bool> CompensateAsync(
        Guid? bookingId,
        BookingSagaState failedAt,
        CancellationToken ct)
    {
        if (!bookingId.HasValue)
        {
            return true;
        }

        try
        {
            _logger.LogWarning(
                "Saga: Compensating booking {Id} after failure at {State}",
                bookingId,
                failedAt);

            await _sender.Send(new CancelPaymentCommand(bookingId.Value), ct);

            _logger.LogInformation("Saga: Compensation completed for booking {Id}", bookingId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga: Compensation failed for booking {Id}", bookingId);
            return false;
        }
    }
}

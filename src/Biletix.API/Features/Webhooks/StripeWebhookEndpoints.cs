using Biletix.API.Common;
using Biletix.Application.Features.Payments.Commands.ConfirmBooking;
using Biletix.Application.Features.Payments.Commands.ExpireBookingOnFailure;
using MediatR;
using Stripe;
using StripeEvent = Stripe.Event;

namespace Biletix.API.Features.Webhooks;

/// <summary>
/// Stripe webhook endpoint'lerini Minimal API uzerinden map eder.
/// </summary>
public sealed class StripeWebhookEndpoints : IEndpoint
{
    /// <summary>
    /// Stripe imzali webhook route'unu tanimlar.
    /// </summary>
    /// <param name="app">Endpoint'lerin eklenecegi route builder.</param>
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/webhooks/stripe", HandleStripeWebhookAsync)
            .AllowAnonymous()
            .WithTags("Webhooks")
            .WithName("StripeWebhook")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpContext httpContext,
        ISender sender,
        IConfiguration configuration,
        ILogger<StripeWebhookEndpoints> logger)
    {
        var webhookSecret = configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            logger.LogError("Stripe webhook secret is not configured");
            return Results.BadRequest("Webhook secret is not configured");
        }

        string payload;
        using (var reader = new StreamReader(httpContext.Request.Body))
        {
            payload = await reader.ReadToEndAsync();
        }

        if (!httpContext.Request.Headers.TryGetValue(
            "Stripe-Signature",
            out var stripeSignature))
        {
            logger.LogWarning("Webhook received without Stripe-Signature header");
            return Results.BadRequest("Missing Stripe-Signature");
        }

        StripeEvent stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                stripeSignature,
                webhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature");
            return Results.BadRequest("Invalid signature");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook payload");
            return Results.BadRequest("Invalid payload");
        }

        logger.LogInformation("Stripe webhook received: {Type}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                if (stripeEvent.Data.Object is PaymentIntent succeededIntent)
                {
                    await sender.Send(new ConfirmBookingCommand(succeededIntent.Id));
                }

                break;

            case "payment_intent.payment_failed":
                if (stripeEvent.Data.Object is PaymentIntent failedIntent)
                {
                    await sender.Send(new ExpireBookingOnPaymentFailureCommand(failedIntent.Id));
                }

                break;

            default:
                logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                break;
        }

        return Results.Ok();
    }
}

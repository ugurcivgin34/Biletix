using Biletix.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Biletix.Infrastructure.Payment;

/// <summary>
/// Stripe uzerinden payment intent olusturma ve iptal islemlerini gerceklestirir.
/// </summary>
public sealed class StripePaymentService : IPaymentService
{
    private readonly StripeClient _stripeClient;

    /// <summary>
    /// Stripe istemcisini konfigurasyondaki secret key ile olusturur.
    /// </summary>
    /// <param name="configuration">Stripe ayarlarini tasiyan konfigurasyon.</param>
    public StripePaymentService(IConfiguration configuration)
    {
        var secretKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        _stripeClient = new StripeClient(secretKey);
    }

    /// <inheritdoc />
    public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        Guid bookingId,
        decimal amount,
        string currency,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = currency,
            Metadata = new Dictionary<string, string> // Payment intent'e rezervasyon kimligi ve kaynak bilgisi ekler.
            {
                { "bookingId", bookingId.ToString() },
                { "source", "biletix" }
            },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true // Stripe'in otomatik ödeme yöntemlerini kullanarak müşterinin uygun ödeme yöntemlerini sunmasını sağlar.
            }
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = idempotencyKey
        };

        var service = new PaymentIntentService(_stripeClient);
        var intent = await service.CreateAsync(options, requestOptions, ct);

        return new CreatePaymentIntentResult(intent.Id, intent.ClientSecret, intent.Status);
    }

    /// <inheritdoc />
    public async Task<PaymentIntentStatusResult> GetPaymentIntentStatusAsync(
        string paymentIntentId,
        CancellationToken ct = default)
    {
        var service = new PaymentIntentService(_stripeClient);
        var intent = await service.GetAsync(paymentIntentId, cancellationToken: ct);

        Guid? bookingId = null;
        if (intent.Metadata.TryGetValue("bookingId", out var bookingIdText) &&
            Guid.TryParse(bookingIdText, out var parsedBookingId))
        {
            bookingId = parsedBookingId;
        }

        return new PaymentIntentStatusResult(intent.Id, intent.Status, bookingId);
    }

    /// <inheritdoc />
    public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        var service = new PaymentIntentService(_stripeClient);
        await service.CancelAsync(paymentIntentId, cancellationToken: ct);

        return true;
    }
}

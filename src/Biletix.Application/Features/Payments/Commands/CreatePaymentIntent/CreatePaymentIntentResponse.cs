namespace Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;

/// <summary>
/// Payment intent olusturma endpoint'inde dondurulen cevap modelidir.
/// </summary>
/// <param name="BookingId">Rezervasyon kimligi.</param>
/// <param name="ClientSecret">Frontend tarafinda odeme tamamlamak icin kullanilan client secret.</param>
/// <param name="PaymentIntentId">Stripe payment intent kimligi.</param>
/// <param name="Amount">Rezervasyon toplam tutari.</param>
/// <param name="ExpiresAt">Rezervasyon gecerlilik bitis zamani.</param>
public sealed record CreatePaymentIntentResponse(
    Guid BookingId,
    string ClientSecret,
    string PaymentIntentId,
    decimal Amount,
    DateTime? ExpiresAt);

namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Odeme saglayicisi ile payment intent operasyonlarini yoneten servis sozlesmesidir.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Rezervasyon icin odeme niyeti olusturur.
    /// </summary>
    /// <param name="bookingId">Odeme alinacak rezervasyon kimligi.</param>
    /// <param name="amount">Odeme tutari.</param>
    /// <param name="currency">Para birimi.</param>
    /// <param name="idempotencyKey">Odeme saglayicisi idempotency anahtari.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    /// <returns>Olusturulan payment intent bilgisi.</returns>
    Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(
        Guid bookingId,
        decimal amount,
        string currency,
        string idempotencyKey,
        CancellationToken ct = default);

    /// <summary>
    /// Odeme niyetini iptal eder.
    /// </summary>
    /// <param name="paymentIntentId">Iptal edilecek payment intent kimligi.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    /// <returns>Iptal basariliysa true.</returns>
    Task<bool> CancelPaymentIntentAsync(string paymentIntentId, CancellationToken ct = default);
}

/// <summary>
/// Odeme niyeti olusturma sonucunu temsil eder.
/// </summary>
/// <param name="PaymentIntentId">Odeme saglayicisindaki payment intent kimligi.</param>
/// <param name="ClientSecret">Frontend tarafinda odeme tamamlamak icin kullanilan client secret.</param>
/// <param name="Status">Payment intent durumu.</param>
public sealed record CreatePaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    string Status);

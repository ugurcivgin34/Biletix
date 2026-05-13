using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Payments.Commands.ExpireBookingOnFailure;

/// <summary>
/// Stripe payment intent basarisiz oldugunda rezervasyonu iptal etmek icin kullanilan komuttur.
/// </summary>
public sealed class ExpireBookingOnPaymentFailureCommand : ICommand
{
    /// <summary>
    /// Komutu bos olusturur.
    /// </summary>
    public ExpireBookingOnPaymentFailureCommand()
    {
    }

    /// <summary>
    /// Komutu payment intent kimligiyle olusturur.
    /// </summary>
    /// <param name="paymentIntentId">Stripe payment intent kimligi.</param>
    public ExpireBookingOnPaymentFailureCommand(string paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
    }

    /// <summary>
    /// Stripe payment intent kimligi.
    /// </summary>
    public string PaymentIntentId { get; set; } = string.Empty;
}

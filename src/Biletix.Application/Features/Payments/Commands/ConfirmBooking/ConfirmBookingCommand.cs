using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Payments.Commands.ConfirmBooking;

/// <summary>
/// Stripe payment intent basarili oldugunda rezervasyonu kesinlestirmek icin kullanilan komuttur.
/// </summary>
public sealed class ConfirmBookingCommand : ICommand
{
    /// <summary>
    /// Komutu bos olusturur.
    /// </summary>
    public ConfirmBookingCommand()
    {
    }

    /// <summary>
    /// Komutu payment intent kimligiyle olusturur.
    /// </summary>
    /// <param name="paymentIntentId">Stripe payment intent kimligi.</param>
    public ConfirmBookingCommand(string paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
    }

    /// <summary>
    /// Stripe payment intent kimligi.
    /// </summary>
    public string PaymentIntentId { get; set; } = string.Empty;
}

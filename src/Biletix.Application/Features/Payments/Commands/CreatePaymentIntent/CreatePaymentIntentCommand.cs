using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;

/// <summary>
/// Pending rezervasyon icin Stripe payment intent olusturmak icin kullanilan komuttur.
/// </summary>
public sealed class CreatePaymentIntentCommand : ICommand<CreatePaymentIntentResponse>
{
    /// <summary>
    /// Komutu bos olusturur.
    /// </summary>
    public CreatePaymentIntentCommand()
    {
    }

    /// <summary>
    /// Komutu rezervasyon kimligiyle olusturur.
    /// </summary>
    /// <param name="bookingId">Odeme niyeti olusturulacak rezervasyon kimligi.</param>
    public CreatePaymentIntentCommand(Guid bookingId)
    {
        BookingId = bookingId;
    }

    /// <summary>
    /// Odeme niyeti olusturulacak rezervasyon kimligi.
    /// </summary>
    public Guid BookingId { get; set; }
}

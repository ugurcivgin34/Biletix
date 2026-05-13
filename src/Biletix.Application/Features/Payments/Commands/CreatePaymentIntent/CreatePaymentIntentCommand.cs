using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;

/// <summary>
/// Pending rezervasyon icin Stripe payment intent olusturmak icin kullanilan komuttur.
/// </summary>
public sealed class CreatePaymentIntentCommand : ICommand<CreatePaymentIntentResponse>
{
    /// <summary>
    /// Odeme niyeti olusturulacak rezervasyon kimligi.
    /// </summary>
    public Guid BookingId { get; set; }
}

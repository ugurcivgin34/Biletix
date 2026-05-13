using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Payments.Commands.CancelPayment;

/// <summary>
/// Pending rezervasyonu ve varsa payment intent'i iptal etmek icin kullanilan komuttur.
/// </summary>
public sealed class CancelPaymentCommand : ICommand
{
    /// <summary>
    /// Iptal edilecek rezervasyon kimligi.
    /// </summary>
    public Guid BookingId { get; set; }
}

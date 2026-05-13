using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Tickets.Commands.ValidateTicket;

/// <summary>
/// QR bilet token'ini kapi girisinde dogrulayan komuttur.
/// </summary>
public sealed class ValidateTicketCommand : ICommand<ValidateTicketResponse>
{
    /// <summary>
    /// QR koddan okunan JWT imzali bilet token'i.
    /// </summary>
    public string QrToken { get; init; } = string.Empty;

    /// <summary>
    /// Taramayi yapan kapi gorevlisi, cihaz veya turnike kimligi.
    /// </summary>
    public string ScannedBy { get; init; } = string.Empty;
}

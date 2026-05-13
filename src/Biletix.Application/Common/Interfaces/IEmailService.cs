namespace Biletix.Application.Common.Interfaces;

/// <summary>
/// Application katmaninin e-posta gonderimi icin kullandigi servis sozlesmesidir.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// HTML icerikli tek bir e-posta gonderir.
    /// </summary>
    /// <param name="toEmail">Alici e-posta adresi.</param>
    /// <param name="toName">Alici gorunen adi.</param>
    /// <param name="subject">E-posta konusu.</param>
    /// <param name="htmlContent">HTML e-posta icerigi.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        CancellationToken ct = default);
}

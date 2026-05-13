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
    /// <param name="inlineAttachments">HTML icinde cid ile referans verilen inline ekler.</param>
    /// <param name="ct">Iptal bildirimi.</param>
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        IReadOnlyCollection<EmailInlineAttachment>? inlineAttachments = null,
        CancellationToken ct = default);
}

/// <summary>
/// E-posta HTML'i icinde cid ile gosterilen inline dosya ekini temsil eder.
/// </summary>
public sealed record EmailInlineAttachment(
    string ContentId,
    string FileName,
    string ContentType,
    byte[] Content);

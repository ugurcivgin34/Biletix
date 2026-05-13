using Biletix.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Biletix.Infrastructure.Notifications;

/// <summary>
/// Gmail SMTP uzerinden HTML e-posta gonderen servis implementasyonudur.
/// </summary>
public sealed class GmailSmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailSmtpEmailService> _logger;

    /// <summary>
    /// Gmail SMTP konfigurasyonu ve logger bagimliliklarini alir.
    /// </summary>
    /// <param name="configuration">Email ayarlarini tasiyan konfigurasyon.</param>
    /// <param name="logger">E-posta loglarini yazan logger.</param>
    public GmailSmtpEmailService(
        IConfiguration configuration,
        ILogger<GmailSmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        IReadOnlyCollection<EmailInlineAttachment>? inlineAttachments = null,
        CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            GetRequiredEmailSetting("FromName"),
            GetRequiredEmailSetting("FromEmail")));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlContent
        };

        if (inlineAttachments is not null)
        {
            foreach (var attachment in inlineAttachments)
            {
                var resource = bodyBuilder.LinkedResources.Add(
                    attachment.FileName,
                    attachment.Content,
                    ContentType.Parse(attachment.ContentType));
                resource.ContentId = attachment.ContentId;
                resource.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            GetRequiredEmailSetting("Host"),
            int.Parse(GetRequiredEmailSetting("Port")),
            SecureSocketOptions.StartTls,
            ct);
        await client.AuthenticateAsync(
            GetRequiredEmailSetting("Username"),
            GetRequiredEmailSetting("Password"),
            ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email sent to {Email} subject: {Subject}", toEmail, subject);
    }

    private string GetRequiredEmailSetting(string key)
    {
        var value = _configuration[$"Email:{key}"];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Email:{key} configuration is required");
        }

        return value;
    }
}

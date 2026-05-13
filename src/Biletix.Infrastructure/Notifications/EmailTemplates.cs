using System.Net;

namespace Biletix.Infrastructure.Notifications;

/// <summary>
/// Bildirim e-postalari icin HTML sablonlarini uretir.
/// </summary>
public static class EmailTemplates
{
    /// <summary>
    /// Onaylanan rezervasyon e-postasi HTML icerigini olusturur.
    /// </summary>
    public static string BookingConfirmed(
        string firstName,
        string eventTitle,
        DateTime eventDate,
        string venueName,
        decimal totalAmount,
        Guid bookingId)
    {
        return Layout(
            "Biletiniz Onaylandı",
            $"""
            <p>Merhaba {Encode(firstName)},</p>
            <p>Rezervasyonunuz başarıyla onaylandı. Etkinlik detaylarınız aşağıdadır.</p>
            <div style="border:1px solid #e5e7eb;border-radius:8px;padding:18px;margin:20px 0;background:#f9fafb;">
              <h2 style="margin:0 0 12px;color:#111827;font-size:20px;">{Encode(eventTitle)}</h2>
              <p style="margin:8px 0;"><strong>Tarih:</strong> {eventDate:dd.MM.yyyy HH:mm}</p>
              <p style="margin:8px 0;"><strong>Mekan:</strong> {Encode(venueName)}</p>
              <p style="margin:8px 0;"><strong>Toplam Tutar:</strong> {totalAmount:N2} TL</p>
              <p style="margin:8px 0;"><strong>Rezervasyon No:</strong> {bookingId}</p>
            </div>
            <p style="color:#4b5563;">QR biletiniz yakında gönderilecek. Etkinlik girişinde bu bileti ve kimliğinizi hazır bulundurunuz.</p>
            """);
    }

    /// <summary>
    /// Suresi dolan rezervasyon e-postasi HTML icerigini olusturur.
    /// </summary>
    public static string BookingExpired(string firstName, string eventTitle)
    {
        return Layout(
            "Rezervasyonunuzun Süresi Doldu",
            $"""
            <p>Merhaba {Encode(firstName)},</p>
            <p><strong>{Encode(eventTitle)}</strong> etkinliği için yaptığınız rezervasyonun süresi doldu.</p>
            <p>Biletler tekrar satışa açıldı. Dilerseniz yeni bir rezervasyon oluşturabilirsiniz.</p>
            <p style="margin:24px 0;">
              <a href="http://localhost:5157" style="background:#111827;color:#ffffff;text-decoration:none;padding:12px 18px;border-radius:6px;display:inline-block;">Tekrar Dene</a>
            </p>
            """);
    }

    /// <summary>
    /// Basarisiz odeme e-postasi HTML icerigini olusturur.
    /// </summary>
    public static string PaymentFailed(string firstName, string eventTitle)
    {
        return Layout(
            "Ödemeniz İşlenemedi",
            $"""
            <p>Merhaba {Encode(firstName)},</p>
            <p><strong>{Encode(eventTitle)}</strong> etkinliği için ödemeniz işlenemedi.</p>
            <p>Kart bilgilerinizi kontrol edin veya farklı bir ödeme yöntemiyle tekrar deneyin.</p>
            """);
    }

    private static string Layout(string title, string body)
    {
        return $"""
        <!doctype html>
        <html lang="tr">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
        </head>
        <body style="margin:0;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;color:#111827;">
          <div style="max-width:640px;margin:0 auto;background:#ffffff;">
            <div style="background:#111827;color:#ffffff;padding:24px 28px;">
              <div style="font-size:24px;font-weight:700;">🎫 Biletix</div>
              <div style="margin-top:8px;color:#d1d5db;">{Encode(title)}</div>
            </div>
            <div style="padding:28px;line-height:1.6;font-size:16px;">
              {body}
            </div>
            <div style="padding:18px 28px;background:#f9fafb;color:#6b7280;font-size:13px;">
              Bu e-posta Biletix rezervasyon sürecinizle ilgili otomatik olarak gönderildi.
            </div>
          </div>
        </body>
        </html>
        """;
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }
}

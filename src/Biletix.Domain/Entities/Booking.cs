using Biletix.Domain.Base;
using Biletix.Domain.Events;
using Biletix.Domain.Exceptions;

namespace Biletix.Domain.Entities;

/// <summary>
/// Kullanicinin etkinlik icin yaptigi bilet rezervasyonu ve odeme surecini temsil eder.
/// </summary>
public class Booking : AggregateRoot<Guid>
{
    private readonly List<BookingItem> _items = new();

    private Booking()
    {
    }

    /// <summary>
    /// Rezervasyonu yapan kullanicinin kimligidir.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Rezervasyonun ait oldugu etkinlik kimligidir.
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// Rezervasyonun ait oldugu etkinlik bilgisidir.
    /// </summary>
    public Event? Event { get; private set; }

    /// <summary>
    /// Rezervasyonun mevcut durumudur.
    /// </summary>
    public BookingStatus Status { get; private set; }

    /// <summary>
    /// Rezervasyon kalemlerinin toplam tutaridir.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Ayni istegin tekrar islenmesini engellemek icin kullanilan anahtardir.
    /// </summary>
    public string? IdempotencyKey { get; private set; }

    /// <summary>
    /// Odeme saglayicisindaki islem kimligidir.
    /// </summary>
    public string? PaymentIntentId { get; private set; }

    /// <summary>
    /// Bekleyen rezervasyonun gecerlilik bitis zamanidir.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Rezervasyondaki bilet kalemlerini disaridan degistirilemeyecek sekilde dondurur.
    /// </summary>
    public IReadOnlyList<BookingItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Pending durumunda ve 10 dakika gecerlilik suresi olan yeni rezervasyon olusturur.
    /// </summary>
    /// <param name="userId">Rezervasyonu yapan kullanici kimligi.</param>
    /// <param name="eventId">Rezervasyonun ait oldugu etkinlik kimligi.</param>
    /// <param name="idempotencyKey">Tekrar eden istekleri ayirt etmek icin kullanilan anahtar.</param>
    /// <returns>Yeni rezervasyon aggregate'i.</returns>
    /// <exception cref="DomainException">Kimlikler veya idempotency key gecersizse firlatilir.</exception>
    public static Booking Create(Guid userId, Guid eventId, string idempotencyKey)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Booking user id cannot be empty");
        }

        if (eventId == Guid.Empty)
        {
            throw new DomainException("Booking event id cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new DomainException("Booking idempotency key cannot be empty");
        }

        var utcNow = DateTime.UtcNow;
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = eventId,
            IdempotencyKey = idempotencyKey,
            Status = BookingStatus.Pending,
            ExpiresAt = utcNow.AddMinutes(10),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        booking.AddDomainEvent(new BookingCreatedDomainEvent(booking.Id, userId));

        return booking;
    }

    /// <summary>
    /// Rezervasyona bilet kalemi ekler ve toplam tutari gunceller.
    /// </summary>
    /// <param name="ticketTypeId">Bilet tipi kimligi.</param>
    /// <param name="quantity">Bilet adedi.</param>
    /// <param name="unitPrice">Birim fiyat.</param>
    /// <exception cref="DomainException">Adet veya fiyat gecersizse firlatilir.</exception>
    public void AddItem(Guid ticketTypeId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Booking item quantity must be greater than zero");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Booking item unit price cannot be negative");
        }

        _items.Add(new BookingItem(Id, ticketTypeId, quantity, unitPrice));
        TotalAmount += quantity * unitPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Pending rezervasyon icin odeme saglayicisi payment intent kimligini kaydeder.
    /// </summary>
    /// <param name="paymentIntentId">Odeme saglayicisindaki payment intent kimligi.</param>
    /// <exception cref="DomainException">Rezervasyon pending degilse veya kimlik bos ise firlatilir.</exception>
    public void SetPaymentIntent(string paymentIntentId)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new DomainException("Only pending bookings can be assigned a payment intent");
        }

        if (string.IsNullOrWhiteSpace(paymentIntentId))
        {
            throw new DomainException("Payment intent id cannot be empty");
        }

        PaymentIntentId = paymentIntentId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bekleyen rezervasyonu odeme bilgisiyle kesinlestirir.
    /// </summary>
    /// <param name="paymentIntentId">Odeme saglayicisindaki islem kimligi.</param>
    /// <exception cref="DomainException">Rezervasyon pending degilse firlatilir.</exception>
    public void Confirm(string paymentIntentId)
    {
        if (Status != BookingStatus.Pending)
        {
            throw new DomainException("Only pending bookings can be confirmed");
        }

        Status = BookingStatus.Confirmed;
        PaymentIntentId = paymentIntentId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingConfirmedDomainEvent(Id, UserId, EventId));
    }

    /// <summary>
    /// Kesinlesmemis rezervasyonu iptal eder.
    /// </summary>
    /// <exception cref="DomainException">Rezervasyon confirmed ise firlatilir.</exception>
    public void Cancel()
    {
        if (Status == BookingStatus.Confirmed)
        {
            throw new DomainException("Confirmed bookings cannot be directly cancelled");
        }

        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingCancelledDomainEvent(Id, UserId));
    }

    /// <summary>
    /// Pending durumundaki rezervasyonu suresi dolmus hale getirir.
    /// </summary>
    public void Expire()
    {
        if (Status != BookingStatus.Pending)
        {
            return;
        }

        Status = BookingStatus.Expired;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingExpiredDomainEvent(Id, UserId));
    }

    /// <summary>
    /// Rezervasyonun gecerlilik suresinin dolup dolmadigini kontrol eder.
    /// </summary>
    /// <returns>Rezervasyon suresi dolduysa true.</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}

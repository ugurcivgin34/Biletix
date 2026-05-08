using Biletix.Domain.Base;
using Biletix.Domain.Events;
using Biletix.Domain.Exceptions;

namespace Biletix.Domain.Entities;

/// <summary>
/// Bilet satilabilen konser, tiyatro veya benzeri etkinlik aggregate'ini temsil eder.
/// </summary>
public class Event : AggregateRoot<Guid>
{
    private readonly List<TicketType> _ticketTypes = new();

    private Event()
    {
    }

    /// <summary>
    /// Etkinligin gorunen basligidir.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Etkinligin detayli aciklamasidir.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Etkinligin baslangic tarihidir.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Etkinligin bitis tarihidir.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Etkinligin gerceklestigi mekanin kimligidir.
    /// </summary>
    public Guid VenueId { get; private set; }

    /// <summary>
    /// Etkinligin gerceklestigi mekan bilgisidir.
    /// </summary>
    public Venue? Venue { get; private set; }

    /// <summary>
    /// Etkinligin performer kimligidir.
    /// </summary>
    public Guid PerformerId { get; private set; }

    /// <summary>
    /// Etkinligin performer bilgisidir.
    /// </summary>
    public Performer? Performer { get; private set; }

    /// <summary>
    /// Etkinligin yayin ve yasam dongusu durumudur.
    /// </summary>
    public EventStatus Status { get; private set; }

    /// <summary>
    /// Etkinlik icin opsiyonel gorsel adresidir.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Etkinlige ait bilet tiplerini disaridan degistirilemeyecek sekilde dondurur.
    /// </summary>
    public IReadOnlyList<TicketType> TicketTypes => _ticketTypes.AsReadOnly();

    /// <summary>
    /// Gecerli alanlarla draft durumunda yeni bir etkinlik aggregate'i olusturur.
    /// </summary>
    /// <param name="title">Etkinlik basligi.</param>
    /// <param name="description">Etkinlik aciklamasi.</param>
    /// <param name="startDate">Etkinlik baslangic tarihi.</param>
    /// <param name="endDate">Etkinlik bitis tarihi.</param>
    /// <param name="venueId">Mekan kimligi.</param>
    /// <param name="performerId">Performer kimligi.</param>
    /// <returns>Yeni etkinlik aggregate'i.</returns>
    /// <exception cref="DomainException">Baslik bos veya tarih araligi gecersizse firlatilir.</exception>
    public static Event Create(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        Guid venueId,
        Guid performerId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Event title cannot be empty");
        }

        if (startDate >= endDate)
        {
            throw new DomainException("Event start date must be before end date");
        }

        var utcNow = DateTime.UtcNow;
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            VenueId = venueId,
            PerformerId = performerId,
            Status = EventStatus.Draft,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        @event.AddDomainEvent(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }

    /// <summary>
    /// Draft durumundaki etkinligi yayina alir.
    /// </summary>
    /// <exception cref="DomainException">Etkinlik draft degilse firlatilir.</exception>
    public void Publish()
    {
        if (Status != EventStatus.Draft)
        {
            throw new DomainException("Only draft events can be published");
        }

        Status = EventStatus.Published;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EventPublishedDomainEvent(Id));
    }

    /// <summary>
    /// Tamamlanmamis etkinligi iptal eder.
    /// </summary>
    /// <exception cref="DomainException">Etkinlik tamamlanmissa firlatilir.</exception>
    public void Cancel()
    {
        if (Status == EventStatus.Completed)
        {
            throw new DomainException("Completed events cannot be cancelled");
        }

        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EventCancelledDomainEvent(Id));
    }
}

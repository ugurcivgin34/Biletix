using Biletix.Domain.Base;

namespace Biletix.Domain.Entities;

/// <summary>
/// Rezervasyon icindeki belirli bir bilet tipine ait adet ve fiyat bilgisini temsil eder.
/// </summary>
public class BookingItem : BaseEntity<Guid>
{
    private BookingItem()
    {
    }

    internal BookingItem(Guid bookingId, Guid ticketTypeId, int quantity, decimal unitPrice)
    {
        var utcNow = DateTime.UtcNow;

        Id = Guid.NewGuid();
        BookingId = bookingId;
        TicketTypeId = ticketTypeId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Kalemin ait oldugu rezervasyon kimligidir.
    /// </summary>
    public Guid BookingId { get; private set; }

    /// <summary>
    /// Kalemin temsil ettigi bilet tipi kimligidir.
    /// </summary>
    public Guid TicketTypeId { get; private set; }

    /// <summary>
    /// Kalemin temsil ettigi bilet tipi bilgisidir.
    /// </summary>
    public TicketType? TicketType { get; private set; }

    /// <summary>
    /// Bu kalemdeki bilet adedidir.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Biletin rezervasyon anindaki birim fiyatidir.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Adet ve birim fiyat carpimindan hesaplanan kalem toplamidir.
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;
}

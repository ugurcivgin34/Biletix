using System.Reflection;
using Biletix.Domain.Entities;

namespace Biletix.Domain.Tests.TestData;

public static class BookingTestData
{
    public static Booking CreateBooking(Guid? userId = null, Guid? eventId = null)
    {
        return Booking.Create(userId ?? Guid.NewGuid(), eventId ?? Guid.NewGuid(), "key-123");
    }

    public static TicketType CreateTicketType(int capacity = 100, decimal price = 500)
    {
        return TicketType.Create(Guid.NewGuid(), "Standart", price, capacity);
    }

    public static Event CreateEvent()
    {
        return Event.Create(
            "Tarkan Konseri",
            "desc",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(3),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    public static Venue CreateVenue()
    {
        return Venue.Create("Arena", "Istanbul", "Address", 10000);
    }

    public static void SetPrivateProperty<T>(object target, string propertyName, T value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property!.SetValue(target, value);
    }
}

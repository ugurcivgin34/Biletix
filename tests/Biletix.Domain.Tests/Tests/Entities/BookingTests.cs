using Biletix.Domain.Entities;
using Biletix.Domain.Events;
using Biletix.Domain.Exceptions;
using Biletix.Domain.Tests.TestData;
using FluentAssertions;

namespace Biletix.Domain.Tests.Tests.Entities;

public class BookingTests
{
    [Fact]
    public void Create_ShouldCreatePendingBooking()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = Booking.Create(userId, eventId, "key-123");

        booking.Status.Should().Be(BookingStatus.Pending);
        booking.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));
        booking.GetDomainEvents().Should().ContainSingle(domainEvent => domainEvent is BookingCreatedDomainEvent);
    }

    [Fact]
    public void Confirm_ShouldConfirmPendingBooking()
    {
        var booking = BookingTestData.CreateBooking();

        booking.Confirm("pi_test_123");

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.PaymentIntentId.Should().Be("pi_test_123");
        booking.GetDomainEvents().Should().Contain(domainEvent => domainEvent is BookingConfirmedDomainEvent);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenNotPending()
    {
        var booking = BookingTestData.CreateBooking();
        booking.Expire();

        var act = () => booking.Confirm("pi_test");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_ShouldCancelPendingBooking()
    {
        var booking = BookingTestData.CreateBooking();

        booking.Cancel();

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.GetDomainEvents().Should().Contain(domainEvent => domainEvent is BookingCancelledDomainEvent);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenConfirmed()
    {
        var booking = BookingTestData.CreateBooking();
        booking.Confirm("pi_test");

        var act = () => booking.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Expire_ShouldExpirePendingBooking()
    {
        var booking = BookingTestData.CreateBooking();

        booking.Expire();

        booking.Status.Should().Be(BookingStatus.Expired);
        booking.GetDomainEvents().Should().Contain(domainEvent => domainEvent is BookingExpiredDomainEvent);
    }

    [Fact]
    public void Expire_ShouldDoNothing_WhenAlreadyExpired()
    {
        var booking = BookingTestData.CreateBooking();

        booking.Expire();
        booking.Expire();

        booking.Status.Should().Be(BookingStatus.Expired);
        booking.GetDomainEvents().Count(domainEvent => domainEvent is BookingExpiredDomainEvent).Should().Be(1);
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExpiresAtPast()
    {
        var booking = BookingTestData.CreateBooking();
        BookingTestData.SetPrivateProperty(booking, nameof(Booking.ExpiresAt), DateTime.UtcNow.AddMinutes(-1));

        booking.IsExpired().Should().BeTrue();
    }

    [Fact]
    public void AddItem_ShouldIncreaseTotalAmount()
    {
        var booking = BookingTestData.CreateBooking();

        booking.AddItem(Guid.NewGuid(), quantity: 2, unitPrice: 500);

        booking.TotalAmount.Should().Be(1000);
        booking.Items.Should().HaveCount(1);
    }
}

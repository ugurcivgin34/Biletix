using Biletix.Domain.Entities;
using Biletix.Domain.Events;
using Biletix.Domain.Exceptions;
using Biletix.Domain.Tests.TestData;
using FluentAssertions;

namespace Biletix.Domain.Tests.Tests.Entities;

public class EventTests
{
    [Fact]
    public void Create_ShouldCreateDraftEvent()
    {
        var @event = Event.Create(
            "Tarkan Konseri",
            "desc",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(3),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        @event.Status.Should().Be(EventStatus.Draft);
        @event.GetDomainEvents().Should().ContainSingle(domainEvent => domainEvent is EventCreatedDomainEvent);
    }

    [Fact]
    public void Create_ShouldThrow_WhenEndDateBeforeStartDate()
    {
        var act = () => Event.Create(
            "Test",
            "desc",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(29),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Publish_ShouldPublishDraftEvent()
    {
        var @event = BookingTestData.CreateEvent();

        @event.Publish();

        @event.Status.Should().Be(EventStatus.Published);
        @event.GetDomainEvents().Should().Contain(domainEvent => domainEvent is EventPublishedDomainEvent);
    }

    [Fact]
    public void Publish_ShouldThrow_WhenNotDraft()
    {
        var @event = BookingTestData.CreateEvent();
        @event.Publish();

        var act = () => @event.Publish();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_ShouldCancelEvent()
    {
        var @event = BookingTestData.CreateEvent();
        @event.Publish();

        @event.Cancel();

        @event.Status.Should().Be(EventStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenCompleted()
    {
        var @event = BookingTestData.CreateEvent();
        BookingTestData.SetPrivateProperty(@event, nameof(Event.Status), EventStatus.Completed);

        var act = () => @event.Cancel();

        act.Should().Throw<DomainException>();
    }
}

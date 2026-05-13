using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Bookings.Commands.ReserveTickets;
using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Application.Tests.TestHelpers;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Biletix.Application.Tests.Tests.Features.Bookings;

public class ReserveTicketsCommandHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task Handle_ShouldReserveTickets_WhenLockAcquired()
    {
        var fixture = CreateFixture();
        fixture.IdempotencyService.GetCachedResponseAsync(fixture.Command.IdempotencyKey).Returns((string?)null);
        fixture.TicketLockService
            .AcquireLockAsync(fixture.TicketType.Id, fixture.UserId, Arg.Any<TimeSpan>())
            .Returns(true);

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.TotalAmount.Should().Be(1000);
        fixture.TicketType.ReservedCount.Should().Be(2);
        await fixture.TicketLockService
            .Received(1)
            .AcquireLockAsync(fixture.TicketType.Id, fixture.UserId, Arg.Any<TimeSpan>());
        await fixture.Context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedResponse_WhenIdempotencyKeyExists()
    {
        var fixture = CreateFixture();
        var cachedResponse = new BookingResponse(
            Guid.NewGuid(),
            fixture.Event.Id,
            fixture.Event.Title,
            BookingStatus.Pending,
            500,
            DateTime.UtcNow.AddMinutes(10),
            new List<BookingItemResponse>());
        fixture.IdempotencyService
            .GetCachedResponseAsync(fixture.Command.IdempotencyKey)
            .Returns(JsonSerializer.Serialize(cachedResponse, JsonOptions));

        var result = await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        result.Should().BeEquivalentTo(cachedResponse);
        await fixture.Context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenLockNotAcquired()
    {
        var fixture = CreateFixture();
        fixture.IdempotencyService.GetCachedResponseAsync(fixture.Command.IdempotencyKey).Returns((string?)null);
        fixture.TicketLockService
            .AcquireLockAsync(fixture.TicketType.Id, fixture.UserId, Arg.Any<TimeSpan>())
            .Returns(false);

        var act = async () => await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*currently being reserved*");
        await fixture.Context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEventNotPublished()
    {
        var fixture = CreateFixture(publishEvent: false);
        fixture.IdempotencyService.GetCachedResponseAsync(fixture.Command.IdempotencyKey).Returns((string?)null);

        var act = async () => await fixture.Handler.Handle(fixture.Command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        await fixture.Context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static TestFixture CreateFixture(bool publishEvent = true)
    {
        var userId = Guid.NewGuid();
        var context = Substitute.For<IApplicationDbContext>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        var ticketLockService = Substitute.For<ITicketLockService>();
        var idempotencyService = Substitute.For<IIdempotencyService>();
        var waitingQueueService = Substitute.For<IWaitingQueueService>();

        var @event = Event.Create(
            "Tarkan Konseri",
            "desc",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(3),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
        if (publishEvent)
        {
            @event.Publish();
        }

        var ticketType = TicketType.Create(@event.Id, "Standart", 500, 100);
        AddTicketType(@event, ticketType);

        var events = new List<Event> { @event };
        var bookings = new List<Booking>();
        var mockEvents = MockDbSetFactory.Create(events);
        var mockBookings = MockDbSetFactory.Create(bookings);
        context.Events.Returns(mockEvents);
        context.Bookings.Returns(mockBookings);
        currentUserService.UserId.Returns(userId);
        waitingQueueService.GetQueueLengthAsync(@event.Id).Returns(0);

        var command = new ReserveTicketsCommand(
            @event.Id,
            new List<ReserveTicketItemDto>
            {
                new(ticketType.Id, 2)
            },
            "key-123");
        var handler = new ReserveTicketsCommandHandler(
            context,
            currentUserService,
            ticketLockService,
            idempotencyService,
            waitingQueueService);

        return new TestFixture(
            context,
            currentUserService,
            ticketLockService,
            idempotencyService,
            waitingQueueService,
            handler,
            command,
            @event,
            ticketType,
            userId);
    }

    private static void AddTicketType(Event @event, TicketType ticketType)
    {
        var field = typeof(Event).GetField("_ticketTypes", BindingFlags.Instance | BindingFlags.NonPublic);
        var ticketTypes = (List<TicketType>)field!.GetValue(@event)!;
        ticketTypes.Add(ticketType);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private sealed record TestFixture(
        IApplicationDbContext Context,
        ICurrentUserService CurrentUserService,
        ITicketLockService TicketLockService,
        IIdempotencyService IdempotencyService,
        IWaitingQueueService WaitingQueueService,
        ReserveTicketsCommandHandler Handler,
        ReserveTicketsCommand Command,
        Event Event,
        TicketType TicketType,
        Guid UserId);
}

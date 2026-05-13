using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Venues.Commands.CreateVenue;
using Biletix.Application.Tests.TestHelpers;
using Biletix.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Biletix.Application.Tests.Tests.Features.Venues;

public class CreateVenueCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateVenue_AndReturnId()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var venues = new List<Venue>();
        var mockVenues = MockDbSetFactory.Create(venues);
        context.Venues.Returns(mockVenues);
        var handler = new CreateVenueCommandHandler(context);
        var command = new CreateVenueCommand
        {
            Name = "Arena",
            City = "Istanbul",
            Address = "Address",
            Capacity = 10000
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        venues.Should().ContainSingle(venue => venue.Id == result);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

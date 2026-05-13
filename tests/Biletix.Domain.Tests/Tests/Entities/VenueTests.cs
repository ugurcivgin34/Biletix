using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using FluentAssertions;

namespace Biletix.Domain.Tests.Tests.Entities;

public class VenueTests
{
    [Fact]
    public void Create_ShouldCreateVenue()
    {
        var venue = Venue.Create("Arena", "Istanbul", "Address", 10000);

        venue.Id.Should().NotBeEmpty();
        venue.Name.Should().Be("Arena");
        venue.City.Should().Be("Istanbul");
        venue.Address.Should().Be("Address");
        venue.Capacity.Should().Be(10000);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameEmpty()
    {
        var act = () => Venue.Create(string.Empty, "Istanbul", "Address", 10000);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenCapacityZero()
    {
        var act = () => Venue.Create("Arena", "Istanbul", "Address", 0);

        act.Should().Throw<DomainException>();
    }
}

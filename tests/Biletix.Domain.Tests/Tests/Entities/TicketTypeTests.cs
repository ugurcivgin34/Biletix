using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using FluentAssertions;

namespace Biletix.Domain.Tests.Tests.Entities;

public class TicketTypeTests
{
    [Fact]
    public void Reserve_ShouldIncreaseReservedCount()
    {
        var ticketType = TicketType.Create(Guid.NewGuid(), "Standart", 500, 100);

        ticketType.Reserve(2);

        ticketType.ReservedCount.Should().Be(2);
        ticketType.AvailableCount.Should().Be(98);
    }

    [Fact]
    public void Reserve_ShouldThrow_WhenNotEnoughAvailable()
    {
        var ticketType = TicketType.Create(Guid.NewGuid(), "Standart", 500, 2);

        var act = () => ticketType.Reserve(3);

        act.Should().Throw<DomainException>().WithMessage("*Not enough*");
    }

    [Fact]
    public void ReleaseReservation_ShouldDecreaseReservedCount()
    {
        var ticketType = TicketType.Create(Guid.NewGuid(), "Standart", 500, 100);
        ticketType.Reserve(5);

        ticketType.ReleaseReservation(3);

        ticketType.ReservedCount.Should().Be(2);
    }

    [Fact]
    public void ReleaseReservation_ShouldNotGoBelowZero()
    {
        var ticketType = TicketType.Create(Guid.NewGuid(), "Standart", 500, 100);
        ticketType.Reserve(2);

        ticketType.ReleaseReservation(10);

        ticketType.ReservedCount.Should().Be(0);
    }

    [Fact]
    public void ConfirmSale_ShouldMoveFromReservedToSold()
    {
        var ticketType = TicketType.Create(Guid.NewGuid(), "Standart", 500, 100);
        ticketType.Reserve(3);

        ticketType.ConfirmSale(3);

        ticketType.ReservedCount.Should().Be(0);
        ticketType.SoldCount.Should().Be(3);
    }

    [Fact]
    public void AvailableCount_ShouldBeCorrect()
    {
        var ticketType = TicketType.Create(Guid.NewGuid(), "VIP", 2000, 100);
        ticketType.Reserve(10);
        ticketType.ConfirmSale(5);

        ticketType.AvailableCount.Should().Be(90);
    }
}

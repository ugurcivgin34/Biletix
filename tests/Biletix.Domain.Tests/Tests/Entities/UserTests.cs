using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using FluentAssertions;

namespace Biletix.Domain.Tests.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ShouldCreateCustomerUser()
    {
        var user = User.Create("Customer@Example.com", "Ada", "Lovelace", "hash");

        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("customer@example.com");
        user.Role.Should().Be(UserRole.Customer);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmailInvalid()
    {
        var act = () => User.Create("not-an-email", "Ada", "Lovelace", "hash");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateAdmin_ShouldCreateAdminUser()
    {
        var user = User.CreateAdmin("admin@example.com", "Admin", "User", "hash");

        user.Role.Should().Be(UserRole.Admin);
        user.Email.Should().Be("admin@example.com");
        user.IsActive.Should().BeTrue();
    }
}

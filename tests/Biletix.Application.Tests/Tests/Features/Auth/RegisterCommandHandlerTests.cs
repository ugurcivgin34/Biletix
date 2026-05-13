using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Auth.Commands.Register;
using Biletix.Application.Tests.TestHelpers;
using Biletix.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace Biletix.Application.Tests.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenEmailNotExists()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var authService = Substitute.For<IAuthService>();
        var users = new List<User>();
        var command = CreateCommand();
        var mockUsers = MockDbSetFactory.Create(users);

        context.Users.Returns(mockUsers);
        authService.HashPasswordAsync(command.Password).Returns("hashed-password");
        var handler = new RegisterCommandHandler(context, authService);

        var result = await handler.Handle(command, CancellationToken.None);

        result.UserId.Should().NotBeEmpty();
        result.Email.Should().Be("customer@example.com");
        users.Should().ContainSingle(user => user.Email == "customer@example.com");
        await authService.Received(1).HashPasswordAsync(command.Password);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var authService = Substitute.For<IAuthService>();
        var users = new List<User>
        {
            User.Create("customer@example.com", "Existing", "User", "hash")
        };
        var mockUsers = MockDbSetFactory.Create(users);
        context.Users.Returns(mockUsers);
        var handler = new RegisterCommandHandler(context, authService);
        var command = CreateCommand();

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*already registered*");
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static RegisterCommand CreateCommand()
    {
        return new RegisterCommand
        {
            Email = "Customer@Example.com",
            Password = "Password123!",
            FirstName = "Ada",
            LastName = "Lovelace"
        };
    }
}

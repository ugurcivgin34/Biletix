using System.Net;
using System.Net.Http.Json;
using Biletix.Application.Features.Auth.Commands.Login;
using Biletix.Domain.Entities;
using Biletix.Integration.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Integration.Tests.Tests.Auth;

public class AuthEndpointsTests : IntegrationTestBase
{
    public AuthEndpointsTests(BiletixWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldReturn201_WithValidData()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new@test.com",
            password = "Test123!",
            firstName = "New",
            lastName = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await DbContext.Users.AnyAsync(user => user.Email == "new@test.com")).Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldReturn400_WithDuplicateEmail()
    {
        var request = new
        {
            email = "duplicate@test.com",
            password = "Test123!",
            firstName = "Test",
            lastName = "User"
        };
        await Client.PostAsJsonAsync("/api/auth/register", request);

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WithWeakPassword()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "weak@test.com",
            password = "123",
            firstName = "Weak",
            lastName = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturn200_WithValidCredentials()
    {
        await RegisterCustomerAsync("login@test.com");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login@test.com",
            password = "Test123!"
        });
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldReturn422_WithWrongPassword()
    {
        await RegisterCustomerAsync("wrong-password@test.com");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "wrong-password@test.com",
            password = "Wrong123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturn200_WithValidTokens()
    {
        await RegisterCustomerAsync("refresh@test.com");
        var login = await LoginResponseAsync("refresh@test.com", "Test123!");

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            accessToken = login.AccessToken,
            refreshToken = login.RefreshToken
        });
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.RefreshToken.Should().NotBeNullOrWhiteSpace();
        content.RefreshToken.Should().NotBe(login.RefreshToken);
    }

    [Fact]
    public async Task Logout_ShouldReturn204()
    {
        await RegisterCustomerAsync("logout@test.com");
        var login = await LoginResponseAsync("logout@test.com", "Test123!");
        var request = CreateJsonRequest(
            HttpMethod.Post,
            "/api/auth/logout",
            new { refreshToken = login.RefreshToken },
            login.AccessToken);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

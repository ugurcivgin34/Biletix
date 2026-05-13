using System.Net;
using System.Net.Http.Json;
using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Integration.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Integration.Tests.Tests.Bookings;

public class BookingEndpointsTests : IntegrationTestBase
{
    public BookingEndpointsTests(BiletixWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Reserve_ShouldReturn201_WithValidData()
    {
        var customer = await RegisterCustomerAsync("reserve@test.com");
        var setup = await CreatePublishedEventAsync();
        var request = CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            new
            {
                eventId = setup.Event.Id,
                items = new[] { new { ticketTypeId = setup.TicketType.Id, quantity = 2 } }
            },
            customer.Token,
            "reserve-valid-key");

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        DbContext.ChangeTracker.Clear();
        var ticketType = await DbContext.TicketTypes.FirstAsync(ticketType => ticketType.Id == setup.TicketType.Id);
        ticketType.ReservedCount.Should().Be(2);
    }

    [Fact]
    public async Task Reserve_ShouldReturn201_SameResponse_WithSameIdempotencyKey()
    {
        var customer = await RegisterCustomerAsync("reserve-idempotent@test.com");
        var setup = await CreatePublishedEventAsync();
        var body = new
        {
            eventId = setup.Event.Id,
            items = new[] { new { ticketTypeId = setup.TicketType.Id, quantity = 1 } }
        };

        var firstResponse = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            body,
            customer.Token,
            "same-idempotency-key"));
        var secondResponse = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            body,
            customer.Token,
            "same-idempotency-key"));
        var first = await ReadJsonAsync<BookingResponse>(firstResponse);
        var second = await ReadJsonAsync<BookingResponse>(secondResponse);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        second!.Id.Should().Be(first!.Id);
        (await DbContext.Bookings.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Reserve_ShouldReturn422_WhenTicketLocked()
    {
        var customer1 = await RegisterCustomerAsync("reserve-lock-1@test.com");
        var customer2 = await RegisterCustomerAsync("reserve-lock-2@test.com");
        var setup = await CreatePublishedEventAsync();
        var body = new
        {
            eventId = setup.Event.Id,
            items = new[] { new { ticketTypeId = setup.TicketType.Id, quantity = 1 } }
        };
        var firstResponse = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            body,
            customer1.Token,
            "lock-key-1"));

        var secondResponse = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            body,
            customer2.Token,
            "lock-key-2"));

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Reserve_ShouldReturn400_WithoutIdempotencyKey()
    {
        var customer = await RegisterCustomerAsync("reserve-no-key@test.com");
        var setup = await CreatePublishedEventAsync();
        var request = CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            new
            {
                eventId = setup.Event.Id,
                items = new[] { new { ticketTypeId = setup.TicketType.Id, quantity = 1 } }
            },
            customer.Token);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBooking_ShouldReturn403Or422_ForOtherUser()
    {
        var customer1 = await RegisterCustomerAsync("booking-owner@test.com");
        var customer2 = await RegisterCustomerAsync("booking-other@test.com");
        var setup = await CreatePublishedEventAsync();
        var reserveResponse = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/bookings/reserve",
            new
            {
                eventId = setup.Event.Id,
                items = new[] { new { ticketTypeId = setup.TicketType.Id, quantity = 1 } }
            },
            customer1.Token,
            "owner-booking-key"));
        var booking = await ReadJsonAsync<BookingResponse>(reserveResponse);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/bookings/{booking!.Id}");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customer2.Token);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.UnprocessableEntity);
    }
}

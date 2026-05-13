using System.Net;
using System.Net.Http.Json;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Tickets.Commands.ValidateTicket;
using Biletix.Domain.Entities;
using Biletix.Integration.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Biletix.Integration.Tests.Tests.Tickets;

public class TicketValidationTests : IntegrationTestBase
{
    public TicketValidationTests(BiletixWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ValidateTicket_ShouldReturn200_IsValid_ForConfirmedBooking()
    {
        var customer = await RegisterCustomerAsync("ticket-valid@test.com");
        var setup = await CreatePublishedEventAsync();
        var booking = await CreateBookingAsync(
            customer.UserId,
            setup.Event.Id,
            setup.TicketType.Id,
            BookingStatus.Confirmed);
        var token = GenerateTicketToken(booking.Id, customer.UserId, setup.Event.Id);
        var adminToken = await GetAdminTokenAsync();

        var response = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/tickets/validate",
            new { qrToken = token, scannedBy = "Gate-1" },
            adminToken));
        var result = await response.Content.ReadFromJsonAsync<ValidateTicketResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTicket_ShouldReturn200_IsInvalid_ForAlreadyScanned()
    {
        var customer = await RegisterCustomerAsync("ticket-scanned@test.com");
        var setup = await CreatePublishedEventAsync();
        var booking = await CreateBookingAsync(
            customer.UserId,
            setup.Event.Id,
            setup.TicketType.Id,
            BookingStatus.Confirmed);
        var token = GenerateTicketToken(booking.Id, customer.UserId, setup.Event.Id);
        var adminToken = await GetAdminTokenAsync();
        var body = new { qrToken = token, scannedBy = "Gate-1" };

        await Client.SendAsync(CreateJsonRequest(HttpMethod.Post, "/api/tickets/validate", body, adminToken));
        var secondResponse = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/tickets/validate",
            body,
            adminToken));
        var result = await secondResponse.Content.ReadFromJsonAsync<ValidateTicketResponse>();

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.IsValid.Should().BeFalse();
        result.AlreadyScanned.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTicket_ShouldReturn200_IsInvalid_ForPendingBooking()
    {
        var customer = await RegisterCustomerAsync("ticket-pending@test.com");
        var setup = await CreatePublishedEventAsync();
        var booking = await CreateBookingAsync(
            customer.UserId,
            setup.Event.Id,
            setup.TicketType.Id);
        var token = GenerateTicketToken(booking.Id, customer.UserId, setup.Event.Id);
        var adminToken = await GetAdminTokenAsync();

        var response = await Client.SendAsync(CreateJsonRequest(
            HttpMethod.Post,
            "/api/tickets/validate",
            new { qrToken = token, scannedBy = "Gate-1" },
            adminToken));
        var result = await response.Content.ReadFromJsonAsync<ValidateTicketResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.IsValid.Should().BeFalse();
    }

    private string GenerateTicketToken(Guid bookingId, Guid userId, Guid eventId)
    {
        return Services.GetRequiredService<IQrTicketService>()
            .GenerateTicketToken(bookingId, userId, eventId);
    }
}

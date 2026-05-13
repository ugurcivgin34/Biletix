using System.Net;
using System.Net.Http.Json;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Venues.DTOs;
using Biletix.Domain.Entities;
using Biletix.Integration.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Integration.Tests.Tests.Venues;

public class VenueEndpointsTests : IntegrationTestBase
{
    public VenueEndpointsTests(BiletixWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateVenue_ShouldReturn201_WithAdminToken()
    {
        var adminToken = await GetAdminTokenAsync();
        var request = CreateJsonRequest(
            HttpMethod.Post,
            "/api/venues",
            new { name = "Arena", city = "Istanbul", address = "Address", capacity = 10000 },
            adminToken);

        var response = await Client.SendAsync(request);
        var venueId = await response.Content.ReadFromJsonAsync<Guid>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        venueId.Should().NotBeEmpty();
        (await DbContext.Venues.AnyAsync(venue => venue.Id == venueId)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateVenue_ShouldReturn401_WithoutToken()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/venues",
            new { name = "Arena", city = "Istanbul", address = "Address", capacity = 10000 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVenue_ShouldReturn403_WithCustomerToken()
    {
        var customer = await RegisterCustomerAsync("venue-customer@test.com");
        var request = CreateJsonRequest(
            HttpMethod.Post,
            "/api/venues",
            new { name = "Arena", city = "Istanbul", address = "Address", capacity = 10000 },
            customer.Token);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetVenues_ShouldReturn200_WithPagination()
    {
        await DbContext.Venues.AddRangeAsync(
            Venue.Create("Arena 1", "Istanbul", "Address 1", 1000),
            Venue.Create("Arena 2", "Istanbul", "Address 2", 2000),
            Venue.Create("Arena 3", "Istanbul", "Address 3", 3000));
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var response = await Client.GetAsync("/api/venues?page=1&pageSize=2");
        var content = await response.Content.ReadFromJsonAsync<PagedResult<VenueResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content!.Items.Should().HaveCount(2);
        content.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetVenue_ShouldReturn404_WhenDeleted()
    {
        var adminToken = await GetAdminTokenAsync();
        var createRequest = CreateJsonRequest(
            HttpMethod.Post,
            "/api/venues",
            new { name = "Deleted Arena", city = "Istanbul", address = "Address", capacity = 10000 },
            adminToken);
        var createResponse = await Client.SendAsync(createRequest);
        var venueId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/venues/{venueId}");
        deleteRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        await Client.SendAsync(deleteRequest);
        var response = await Client.GetAsync($"/api/venues/{venueId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

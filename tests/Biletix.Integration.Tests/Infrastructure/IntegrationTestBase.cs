using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Auth.Commands.Login;
using Biletix.Domain.Entities;
using Biletix.Infrastructure.Persistence;
using Biletix.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Biletix.Integration.Tests.Infrastructure;

public abstract class IntegrationTestBase :
    IClassFixture<BiletixWebApplicationFactory>,
    IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly IServiceScope _scope;

    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext DbContext;
    protected readonly IConnectionMultiplexer Redis;
    protected readonly IServiceProvider Services;

    protected IntegrationTestBase(BiletixWebApplicationFactory factory)
    {
        Client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        Services = _scope.ServiceProvider;
        DbContext = Services.GetRequiredService<ApplicationDbContext>();
        Redis = Services.GetRequiredService<IConnectionMultiplexer>();
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.MigrateAsync();

        var authService = Services.GetRequiredService<IAuthService>();
        await AdminSeeder.SeedAsync(DbContext, authService);
    }

    public async Task DisposeAsync()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        DbContext.ChangeTracker.Clear();

        DbContext.TicketScans.RemoveRange(await DbContext.TicketScans.IgnoreQueryFilters().ToListAsync());
        DbContext.OutboxMessages.RemoveRange(await DbContext.OutboxMessages.IgnoreQueryFilters().ToListAsync());
        DbContext.BookingItems.RemoveRange(await DbContext.BookingItems.IgnoreQueryFilters().ToListAsync());
        DbContext.Bookings.RemoveRange(await DbContext.Bookings.IgnoreQueryFilters().ToListAsync());
        DbContext.TicketTypes.RemoveRange(await DbContext.TicketTypes.IgnoreQueryFilters().ToListAsync());
        DbContext.Events.RemoveRange(await DbContext.Events.IgnoreQueryFilters().ToListAsync());
        DbContext.Venues.RemoveRange(await DbContext.Venues.IgnoreQueryFilters().ToListAsync());
        DbContext.Performers.RemoveRange(await DbContext.Performers.IgnoreQueryFilters().ToListAsync());

        var usersToRemove = await DbContext.Users
            .IgnoreQueryFilters()
            .Where(user => user.Email != "admin@biletix.com")
            .ToListAsync();
        DbContext.Users.RemoveRange(usersToRemove);

        await DbContext.SaveChangesAsync();

        var db = Redis.GetDatabase();
        await db.ExecuteAsync("FLUSHDB");

        _scope.Dispose();
    }

    protected async Task<LoginResponse> LoginResponseAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    protected async Task<string> LoginAsync(string email, string password)
    {
        var result = await LoginResponseAsync(email, password);
        return result.AccessToken;
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        return await LoginAsync("admin@biletix.com", "Admin123!");
    }

    protected async Task<(string Token, Guid UserId)> RegisterCustomerAsync(string email = "test@test.com")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Test123!",
            firstName = "Test",
            lastName = "User"
        });
        response.EnsureSuccessStatusCode();

        var token = await LoginAsync(email, "Test123!");
        var user = await DbContext.Users.FirstAsync(user => user.Email == email);

        return (token, user.Id);
    }

    protected static HttpRequestMessage CreateJsonRequest(
        HttpMethod method,
        string requestUri,
        object body,
        string? bearerToken = null,
        string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(body)
        };

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        return request;
    }

    protected static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    protected async Task<(Venue Venue, Performer Performer, Event Event, TicketType TicketType)> CreatePublishedEventAsync(
        int capacity = 100,
        decimal price = 500)
    {
        var admin = await DbContext.Users.FirstAsync(user => user.Email == "admin@biletix.com");
        var venue = Venue.Create($"Arena {Guid.NewGuid():N}", "Istanbul", "Address", 10000);
        var performer = Performer.Create($"Performer {Guid.NewGuid():N}", "Pop");
        var @event = Event.Create(
            "Tarkan Konseri",
            "desc",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(30).AddHours(3),
            venue.Id,
            performer.Id,
            admin.Id);
        @event.Publish();

        var ticketType = TicketType.Create(@event.Id, "Standart", price, capacity);

        await DbContext.Venues.AddAsync(venue);
        await DbContext.Performers.AddAsync(performer);
        await DbContext.Events.AddAsync(@event);
        await DbContext.TicketTypes.AddAsync(ticketType);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        return (venue, performer, @event, ticketType);
    }

    protected async Task<Booking> CreateBookingAsync(
        Guid userId,
        Guid eventId,
        Guid ticketTypeId,
        BookingStatus status = BookingStatus.Pending)
    {
        var booking = Booking.Create(userId, eventId, $"booking-{Guid.NewGuid():N}");
        booking.AddItem(ticketTypeId, quantity: 1, unitPrice: 500);

        if (status == BookingStatus.Confirmed)
        {
            booking.Confirm($"pi_{Guid.NewGuid():N}");
        }
        else if (status == BookingStatus.Expired)
        {
            booking.Expire();
        }
        else if (status == BookingStatus.Cancelled)
        {
            booking.Cancel();
        }

        await DbContext.Bookings.AddAsync(booking);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        return booking;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}

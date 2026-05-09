using System.Text.Json;
using System.Text.Json.Serialization;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Bookings.DTOs;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Bookings.Commands.ReserveTickets;

/// <summary>
/// Redis dagitik kilidi ve idempotency cache'i ile bilet rezervasyon komutunu isler.
/// </summary>
public sealed class ReserveTicketsCommandHandler : ICommandHandler<ReserveTicketsCommand, BookingResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITicketLockService _ticketLockService;
    private readonly IIdempotencyService _idempotencyService;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani, kullanici, lock ve idempotency servislerini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici servisi.</param>
    /// <param name="ticketLockService">Bilet tipi dagitik kilit servisi.</param>
    /// <param name="idempotencyService">Idempotency cache servisi.</param>
    public ReserveTicketsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITicketLockService ticketLockService,
        IIdempotencyService idempotencyService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ticketLockService = ticketLockService;
        _idempotencyService = idempotencyService;
    }

    /// <summary>
    /// Idempotency cache kontrolu yapar, bilet tipi kilitlerini alir, rezervasyonu olusturur ve cevabi cache'ler.
    /// </summary>
    /// <param name="request">Bilet rezervasyon komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Olusturulan veya cache'ten okunan rezervasyon cevabi.</returns>
    public async Task<BookingResponse> Handle(ReserveTicketsCommand request, CancellationToken cancellationToken)
    {
        var cached = await _idempotencyService.GetCachedResponseAsync(request.IdempotencyKey);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<BookingResponse>(cached, JsonOptions)!;
        }

        var userId = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var @event = await _context.Events
            .Include(item => item.TicketTypes)
            .FirstOrDefaultAsync(item => item.Id == request.EventId, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Event", request.EventId);
        }

        if (@event.Status != EventStatus.Published)
        {
            throw new DomainException("Event is not available for booking");
        }

        var lockedTicketTypeIds = new List<Guid>();

        try
        {
            await AcquireLocksAsync(request, @event, userId, lockedTicketTypeIds);
            ReserveTicketCounts(request, @event);

            var booking = Booking.Create(userId, request.EventId, request.IdempotencyKey);
            AddBookingItems(request, @event, booking);

            await _context.Bookings.AddAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = booking.ToResponse(@event);
            await _idempotencyService.CacheResponseAsync(
                request.IdempotencyKey,
                JsonSerializer.Serialize(response, JsonOptions),
                TimeSpan.FromHours(24));

            return response;
        }
        catch
        {
            if (lockedTicketTypeIds.Count > 0 && !await _idempotencyService.ExistsAsync(request.IdempotencyKey))
            {
                foreach (var ticketTypeId in lockedTicketTypeIds)
                {
                    await _ticketLockService.ReleaseLockAsync(ticketTypeId, userId);
                }
            }

            throw;
        }
    }

    private async Task AcquireLocksAsync(
        ReserveTicketsCommand request,
        Event @event,
        Guid userId,
        List<Guid> lockedTicketTypeIds)
    {
        foreach (var item in request.Items)
        {
            var ticketType = GetTicketType(@event, item.TicketTypeId);
            var acquired = await _ticketLockService.AcquireLockAsync(
                item.TicketTypeId,
                userId,
                TimeSpan.FromMinutes(10));

            if (!acquired)
            {
                foreach (var lockedId in lockedTicketTypeIds)
                {
                    await _ticketLockService.ReleaseLockAsync(lockedId, userId);
                }

                throw new DomainException(
                    $"Ticket type '{ticketType.Name}' is currently being reserved by another user. Please try again.");
            }

            lockedTicketTypeIds.Add(item.TicketTypeId);
        }
    }

    private static void ReserveTicketCounts(ReserveTicketsCommand request, Event @event)
    {
        foreach (var item in request.Items)
        {
            var ticketType = GetTicketType(@event, item.TicketTypeId);
            ticketType.Reserve(item.Quantity);
        }
    }

    private static void AddBookingItems(ReserveTicketsCommand request, Event @event, Booking booking)
    {
        foreach (var item in request.Items)
        {
            var ticketType = GetTicketType(@event, item.TicketTypeId);
            booking.AddItem(ticketType.Id, item.Quantity, ticketType.Price);
        }
    }

    private static TicketType GetTicketType(Event @event, Guid ticketTypeId)
    {
        return @event.TicketTypes.FirstOrDefault(ticketType => ticketType.Id == ticketTypeId)
            ?? throw new NotFoundException("TicketType", ticketTypeId);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}

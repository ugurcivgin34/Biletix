using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Etkinlik olusturma komutunu isler.
/// </summary>
public sealed class CreateEventCommandHandler : ICommandHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="currentUserService">Aktif kullanici bilgisi servisi.</param>
    public CreateEventCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Mekan ve performer varligini dogrular, etkinligi bilet tipleriyle birlikte kaydeder.
    /// </summary>
    /// <param name="request">Etkinlik olusturma komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Olusturulan etkinligin kimligi.</returns>
    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var createdBy = _currentUserService.UserId
            ?? throw new DomainException("Authenticated user is required");

        var venueExists = await _context.Venues
            .AnyAsync(venue => venue.Id == request.VenueId, cancellationToken);

        if (!venueExists)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        var performerExists = await _context.Performers
            .AnyAsync(performer => performer.Id == request.PerformerId, cancellationToken);

        if (!performerExists)
        {
            throw new NotFoundException("Performer", request.PerformerId);
        }

        var @event = Event.Create(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.VenueId,
            request.PerformerId,
            createdBy,
            request.ImageUrl);

        await _context.Events.AddAsync(@event, cancellationToken);

        foreach (var ticketTypeDto in request.TicketTypes)
        {
            var ticketType = TicketType.Create(
                @event.Id,
                ticketTypeDto.Name,
                ticketTypeDto.Price,
                ticketTypeDto.TotalCapacity);

            await _context.TicketTypes.AddAsync(ticketType, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return @event.Id;
    }
}

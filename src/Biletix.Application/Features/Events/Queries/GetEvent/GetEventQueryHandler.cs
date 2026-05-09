using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Events.DTOs;
using Biletix.Application.Features.Events.Mappers;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Queries.GetEvent;

/// <summary>
/// Tek etkinlik detay sorgusunu isler.
/// </summary>
public sealed class GetEventQueryHandler : IQueryHandler<GetEventQuery, EventResponse>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public GetEventQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Etkinligi mekan, performer ve bilet tipleriyle birlikte okur.
    /// </summary>
    /// <param name="request">Etkinlik detay sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Etkinlik detay cevap modeli.</returns>
    public async Task<EventResponse> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .Include(item => item.Venue)
            .Include(item => item.Performer)
            .Include(item => item.TicketTypes)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Event", request.Id);
        }

        return @event.ToResponse();
    }
}

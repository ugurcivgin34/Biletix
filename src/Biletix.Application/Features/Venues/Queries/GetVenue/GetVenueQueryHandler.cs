using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Venues.DTOs;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Venues.Queries.GetVenue;

/// <summary>
/// Tek mekan detay sorgusunu isler.
/// </summary>
public sealed class GetVenueQueryHandler : IQueryHandler<GetVenueQuery, VenueResponse>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public GetVenueQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mekani takip edilmeyen sorgu ile okur ve cevap modeline donusturur.
    /// </summary>
    /// <param name="request">Mekan detay sorgusu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Mekan cevap modeli.</returns>
    public async Task<VenueResponse> Handle(GetVenueQuery request, CancellationToken cancellationToken)
    {
        var venue = await _context.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue", request.Id);
        }

        return venue.ToResponse();
    }
}

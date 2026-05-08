using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Venues.Commands.UpdateVenue;

/// <summary>
/// Mekan guncelleme komutunu isler.
/// </summary>
public sealed class UpdateVenueCommandHandler : ICommandHandler<UpdateVenueCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public UpdateVenueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mekani bulur, alanlarini gunceller ve degisiklikleri kaydeder.
    /// </summary>
    /// <param name="request">Mekan guncelleme komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(UpdateVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue", request.Id);
        }

        venue.Name = request.Name;
        venue.City = request.City;
        venue.Address = request.Address;
        venue.Capacity = request.Capacity;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

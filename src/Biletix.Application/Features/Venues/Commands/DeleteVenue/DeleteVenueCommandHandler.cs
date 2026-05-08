using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Venues.Commands.DeleteVenue;

/// <summary>
/// Mekan silme komutunu soft delete olarak uygular.
/// </summary>
public sealed class DeleteVenueCommandHandler : ICommandHandler<DeleteVenueCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public DeleteVenueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mekani bulur ve global query filter'in devreye girecegi sekilde silinmis isaretler.
    /// </summary>
    /// <param name="request">Mekan silme komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(DeleteVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue", request.Id);
        }

        venue.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

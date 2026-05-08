using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;

namespace Biletix.Application.Features.Venues.Commands.CreateVenue;

/// <summary>
/// Mekan olusturma komutunu isler ve yeni mekan kimligini dondurur.
/// </summary>
public sealed class CreateVenueCommandHandler : ICommandHandler<CreateVenueCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public CreateVenueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gecerli bilgilerle yeni mekan olusturur ve kalici hale getirir.
    /// </summary>
    /// <param name="request">Mekan olusturma komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Olusturulan mekanin kimligi.</returns>
    public async Task<Guid> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = Venue.Create(
            request.Name,
            request.City,
            request.Address,
            request.Capacity);

        await _context.Venues.AddAsync(venue, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return venue.Id;
    }
}

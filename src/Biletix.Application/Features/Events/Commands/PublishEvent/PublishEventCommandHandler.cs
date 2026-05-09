using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Commands.PublishEvent;

/// <summary>
/// Etkinlik yayina alma komutunu isler.
/// </summary>
public sealed class PublishEventCommandHandler : ICommandHandler<PublishEventCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IResourceAuthorizationService _resourceAuthorizationService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="resourceAuthorizationService">Kaynak bazli yetkilendirme servisi.</param>
    public PublishEventCommandHandler(
        IApplicationDbContext context,
        IResourceAuthorizationService resourceAuthorizationService)
    {
        _context = context;
        _resourceAuthorizationService = resourceAuthorizationService;
    }

    /// <summary>
    /// Etkinligi bulur, yetkiyi kontrol eder ve domain davranisi uzerinden yayina alir.
    /// </summary>
    /// <param name="request">Etkinlik yayina alma komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Event", request.Id);
        }

        if (!_resourceAuthorizationService.CanManageEvent(@event.CreatedBy))
        {
            throw new DomainException("Access denied");
        }

        @event.Publish();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

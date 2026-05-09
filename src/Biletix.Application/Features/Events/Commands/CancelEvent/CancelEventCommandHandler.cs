using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Commands.CancelEvent;

/// <summary>
/// Etkinlik iptal komutunu isler.
/// </summary>
public sealed class CancelEventCommandHandler : ICommandHandler<CancelEventCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IResourceAuthorizationService _resourceAuthorizationService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="resourceAuthorizationService">Kaynak bazli yetkilendirme servisi.</param>
    public CancelEventCommandHandler(
        IApplicationDbContext context,
        IResourceAuthorizationService resourceAuthorizationService)
    {
        _context = context;
        _resourceAuthorizationService = resourceAuthorizationService;
    }

    /// <summary>
    /// Etkinligi bulur, yetkiyi kontrol eder ve domain davranisi uzerinden iptal eder.
    /// </summary>
    /// <param name="request">Etkinlik iptal komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(CancelEventCommand request, CancellationToken cancellationToken)
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

        @event.Cancel();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

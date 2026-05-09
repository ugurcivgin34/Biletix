using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Commands.UpdateEvent;

/// <summary>
/// Etkinlik guncelleme komutunu isler.
/// </summary>
public sealed class UpdateEventCommandHandler : ICommandHandler<UpdateEventCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IResourceAuthorizationService _resourceAuthorizationService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="resourceAuthorizationService">Kaynak bazli yetkilendirme servisi.</param>
    public UpdateEventCommandHandler(
        IApplicationDbContext context,
        IResourceAuthorizationService resourceAuthorizationService)
    {
        _context = context;
        _resourceAuthorizationService = resourceAuthorizationService;
    }

    /// <summary>
    /// Draft durumundaki etkinligi kaynak sahipligi kontrolunden sonra gunceller.
    /// </summary>
    /// <param name="request">Etkinlik guncelleme komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Event", request.Id);
        }

        if (@event.Status != EventStatus.Draft)
        {
            throw new DomainException("Only draft events can be updated");
        }

        if (!_resourceAuthorizationService.CanManageEvent(@event.CreatedBy))
        {
            throw new DomainException("Access denied");
        }

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.StartDate = request.StartDate;
        @event.EndDate = request.EndDate;
        @event.ImageUrl = request.ImageUrl;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Events.Commands.AddTicketType;

/// <summary>
/// Bilet tipi ekleme komutunu isler.
/// </summary>
public sealed class AddTicketTypeCommandHandler : ICommandHandler<AddTicketTypeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IResourceAuthorizationService _resourceAuthorizationService;

    /// <summary>
    /// Handler'in ihtiyac duydugu servisleri alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    /// <param name="resourceAuthorizationService">Kaynak bazli yetkilendirme servisi.</param>
    public AddTicketTypeCommandHandler(
        IApplicationDbContext context,
        IResourceAuthorizationService resourceAuthorizationService)
    {
        _context = context;
        _resourceAuthorizationService = resourceAuthorizationService;
    }

    /// <summary>
    /// Etkinlik uygunsa yeni bilet tipini olusturur ve kalici hale getirir.
    /// </summary>
    /// <param name="request">Bilet tipi ekleme komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Olusturulan bilet tipinin kimligi.</returns>
    public async Task<Guid> Handle(AddTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .FirstOrDefaultAsync(item => item.Id == request.EventId, cancellationToken);

        if (@event is null)
        {
            throw new NotFoundException("Event", request.EventId);
        }

        if (@event.Status is EventStatus.Cancelled or EventStatus.Completed)
        {
            throw new DomainException("Ticket types cannot be added to cancelled or completed events");
        }

        if (!_resourceAuthorizationService.CanManageEvent(@event.CreatedBy))
        {
            throw new DomainException("Access denied");
        }

        var ticketType = TicketType.Create(
            @event.Id,
            request.Name,
            request.Price,
            request.TotalCapacity);

        await _context.TicketTypes.AddAsync(ticketType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ticketType.Id;
    }
}

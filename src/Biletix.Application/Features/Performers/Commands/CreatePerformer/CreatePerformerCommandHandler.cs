using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;

namespace Biletix.Application.Features.Performers.Commands.CreatePerformer;

/// <summary>
/// Performer olusturma komutunu isler ve yeni performer kimligini dondurur.
/// </summary>
public sealed class CreatePerformerCommandHandler : ICommandHandler<CreatePerformerCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public CreatePerformerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gecerli bilgilerle yeni performer olusturur ve kalici hale getirir.
    /// </summary>
    /// <param name="request">Performer olusturma komutu.</param>
    /// <param name="cancellationToken">Iptal bildirimi.</param>
    /// <returns>Olusturulan performer kimligi.</returns>
    public async Task<Guid> Handle(CreatePerformerCommand request, CancellationToken cancellationToken)
    {
        var performer = Performer.Create(request.Name, request.Genre);
        performer.SetImageUrl(request.ImageUrl);

        await _context.Performers.AddAsync(performer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return performer.Id;
    }
}

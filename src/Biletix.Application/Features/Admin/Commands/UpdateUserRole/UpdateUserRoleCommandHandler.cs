using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Admin.Commands.UpdateUserRole;

/// <summary>
/// Kullanici rol guncelleme komutunu isler.
/// </summary>
public sealed class UpdateUserRoleCommandHandler : ICommandHandler<UpdateUserRoleCommand>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public UpdateUserRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kullaniciyi bulur, rol degerini dogrular ve kaydeder.
    /// </summary>
    public async Task Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new DomainException("Invalid user role");
        }

        user.SetRole(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

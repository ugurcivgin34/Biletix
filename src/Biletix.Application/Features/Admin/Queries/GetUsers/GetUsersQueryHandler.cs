using Biletix.Application.Common.Interfaces;
using Biletix.Application.Common.Models;
using Biletix.Application.Features.Admin.DTOs;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Admin.Queries.GetUsers;

/// <summary>
/// Kullanici listeleme sorgusunu filtreleme ve sayfalama kurallariyla isler.
/// </summary>
public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, PagedResult<UserResponse>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Handler'in ihtiyac duydugu veritabani baglamini alir.
    /// </summary>
    /// <param name="context">Uygulama veritabani baglami.</param>
    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kullanicilari opsiyonel rol filtresiyle listeler.
    /// </summary>
    public async Task<PagedResult<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                throw new DomainException("Invalid user role");
            }

            query = query.Where(user => user.Role == role);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role.ToString(),
                user.IsActive,
                user.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserResponse>(users, totalCount, page, pageSize);
    }
}

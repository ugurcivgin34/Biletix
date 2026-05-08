using System.Security.Claims;
using Biletix.Application.Common.Interfaces;
using Biletix.Application.Features.Auth.Commands.Login;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Refresh token rotasyonu ile yeni token cifti ureten handler'dir.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private static readonly TimeSpan RefreshTokenExpiry = TimeSpan.FromDays(7);

    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    /// <summary>
    /// Refresh token handler icin gerekli veritabani ve token servislerini alir.
    /// </summary>
    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    /// <summary>
    /// Eski refresh token'i iptal eder, yeni access token ve refresh token uretir.
    /// </summary>
    /// <param name="request">Refresh token komutu.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Yeni token cifti iceren cevap modeli.</returns>
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdValue = principal?.FindFirst("sub")?.Value
            ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new DomainException("Invalid access token");
        }

        var refreshTokenValid = await _refreshTokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);

        if (!refreshTokenValid)
        {
            throw new DomainException("Invalid refresh token");
        }

        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!user.IsActive)
        {
            throw new DomainException("Account is deactivated");
        }

        await _refreshTokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _refreshTokenService.StoreRefreshTokenAsync(userId, newRefreshToken, RefreshTokenExpiry);

        return LoginCommandHandler.CreateResponse(user, newAccessToken, newRefreshToken);
    }
}

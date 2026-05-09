using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using Biletix.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Auth.Commands.Login;

/// <summary>
/// Kullanici giris komutunu isleyen handler'dir.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private static readonly TimeSpan RefreshTokenExpiry = TimeSpan.FromDays(7);
    private static readonly TimeSpan AccessTokenExpiry = TimeSpan.FromMinutes(15);

    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    /// <summary>
    /// Login handler icin gerekli veritabani, sifre ve token servislerini alir.
    /// </summary>
    public LoginCommandHandler(
        IApplicationDbContext context,
        IAuthService authService,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _authService = authService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    /// <summary>
    /// Kullanici kimlik bilgilerini dogrular, token uretir ve refresh token'i saklar.
    /// </summary>
    /// <param name="request">Giris komutu.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Access token ve refresh token iceren cevap modeli.</returns>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant(); // E-posta adresini normalize et (bosluklari kaldir, kucuk harfe cevir)
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), normalizedEmail);
        }

        var passwordValid = await _authService.VerifyPasswordAsync(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new DomainException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new DomainException("Account is deactivated");
        }

        user.UpdateLastLogin();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _refreshTokenService.StoreRefreshTokenAsync(user.Id, refreshToken, RefreshTokenExpiry);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateResponse(user, accessToken, refreshToken);
    }

    internal static LoginResponse CreateResponse(User user, string accessToken, string refreshToken)
    {
        return new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.Add(AccessTokenExpiry),
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}",
            user.Role.ToString());
    }
}

using Biletix.Application.Common.Interfaces;

namespace Biletix.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Kullanici cikis komutunu isleyen handler'dir.
/// </summary>
public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IRefreshTokenService _refreshTokenService;

    /// <summary>
    /// Logout handler icin refresh token servisini alir.
    /// </summary>
    /// <param name="refreshTokenService">Refresh token iptal servisi.</param>
    public LogoutCommandHandler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    /// <summary>
    /// Kullaniciya ait belirli refresh token'i iptal eder.
    /// </summary>
    /// <param name="request">Logout komutu.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(request.UserId, request.RefreshToken);
    }
}

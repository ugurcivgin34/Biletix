using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Biletix.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QRCoder;

namespace Biletix.Infrastructure.Tickets;

/// <summary>
/// JWT imzali QR bilet token'i ve PNG QR kod ureten servis implementasyonudur.
/// </summary>
public sealed class QrTicketService : IQrTicketService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<QrTicketService> _logger;

    /// <summary>
    /// QR bilet servisinin ihtiyac duydugu konfigurasyon ve logger bagimliliklarini alir.
    /// </summary>
    public QrTicketService(
        IConfiguration configuration,
        ILogger<QrTicketService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public string GenerateTicketToken(Guid bookingId, Guid userId, Guid eventId)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim("bookingId", bookingId.ToString()),
            new Claim("userId", userId.ToString()),
            new Claim("eventId", eventId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddYears(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public QrTicketClaims? ValidateTicketToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var issuedAtUnix = principal.FindFirstValue(JwtRegisteredClaimNames.Iat)
                ?? principal.FindFirstValue("iat");

            return new QrTicketClaims(
                Guid.Parse(principal.FindFirstValue("bookingId")!),
                Guid.Parse(principal.FindFirstValue("userId")!),
                Guid.Parse(principal.FindFirstValue("eventId")!),
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(issuedAtUnix!)).UtcDateTime,
                jwtToken.ValidTo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid ticket token");
            return null;
        }
    }

    /// <inheritdoc />
    public byte[] GenerateQrCodePng(string token)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);

        return qrCode.GetGraphic(10);
    }
}

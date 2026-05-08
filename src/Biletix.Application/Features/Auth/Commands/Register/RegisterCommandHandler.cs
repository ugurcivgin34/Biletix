using Biletix.Application.Common.Interfaces;
using Biletix.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Biletix.Application.Features.Auth.Commands.Register;

/// <summary>
/// Kullanici kayit komutunu isleyen handler'dir.
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    /// <summary>
    /// Register handler icin veritabani ve sifreleme servislerini alir.
    /// </summary>
    /// <param name="context">Application DbContext sozlesmesi.</param>
    /// <param name="authService">Sifre hashleme servisi.</param>
    public RegisterCommandHandler(IApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    /// <summary>
    /// E-posta benzersizligini kontrol eder, sifreyi hashler ve yeni kullaniciyi kaydeder.
    /// </summary>
    /// <param name="request">Kayit komutu.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Olusturulan kullanicinin cevap modeli.</returns>
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Email), "Email already registered")
            });
        }

        var passwordHash = await _authService.HashPasswordAsync(request.Password);
        var user = User.Create(normalizedEmail, request.FirstName, request.LastName, passwordHash);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.Email, user.FirstName, user.LastName);
    }
}

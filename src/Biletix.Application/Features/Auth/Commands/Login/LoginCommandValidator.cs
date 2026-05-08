using FluentValidation;

namespace Biletix.Application.Features.Auth.Commands.Login;

/// <summary>
/// Kullanici giris komutu icin giris dogrulama kurallarini tanimlar.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// LoginCommand validator kurallarini olusturur.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}

using FluentValidation;

namespace Biletix.Application.Features.Auth.Commands.Register;

/// <summary>
/// Kullanici kayit komutu icin giris dogrulama kurallarini tanimlar.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// RegisterCommand validator kurallarini olusturur.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("Password must contain uppercase, lowercase and digit characters");

        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}

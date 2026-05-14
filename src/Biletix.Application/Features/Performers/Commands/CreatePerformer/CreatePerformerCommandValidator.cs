using FluentValidation;

namespace Biletix.Application.Features.Performers.Commands.CreatePerformer;

/// <summary>
/// Performer olusturma komutunun giris kurallarini dogrular.
/// </summary>
public sealed class CreatePerformerCommandValidator : AbstractValidator<CreatePerformerCommand>
{
    /// <summary>
    /// Performer olusturma validasyon kurallarini tanimlar.
    /// </summary>
    public CreatePerformerCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Genre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.ImageUrl)
            .MaximumLength(500);
    }
}

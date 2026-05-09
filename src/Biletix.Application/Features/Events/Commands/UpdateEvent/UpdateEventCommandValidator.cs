using FluentValidation;

namespace Biletix.Application.Features.Events.Commands.UpdateEvent;

/// <summary>
/// Etkinlik guncelleme komutunun giris kurallarini dogrular.
/// </summary>
public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    /// <summary>
    /// Etkinlik guncelleme validasyon kurallarini tanimlar.
    /// </summary>
    public UpdateEventCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(command => command.StartDate)
            .Must(startDate => startDate > DateTime.UtcNow)
            .WithMessage("StartDate must be in the future.");

        RuleFor(command => command.EndDate)
            .Must((command, endDate) => endDate > command.StartDate)
            .WithMessage("EndDate must be greater than StartDate.");
    }
}

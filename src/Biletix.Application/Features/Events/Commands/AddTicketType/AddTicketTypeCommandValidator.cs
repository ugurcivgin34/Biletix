using FluentValidation;

namespace Biletix.Application.Features.Events.Commands.AddTicketType;

/// <summary>
/// Bilet tipi ekleme komutunun giris kurallarini dogrular.
/// </summary>
public sealed class AddTicketTypeCommandValidator : AbstractValidator<AddTicketTypeCommand>
{
    /// <summary>
    /// Bilet tipi ekleme validasyon kurallarini tanimlar.
    /// </summary>
    public AddTicketTypeCommandValidator()
    {
        RuleFor(command => command.EventId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.TotalCapacity)
            .GreaterThan(0);
    }
}

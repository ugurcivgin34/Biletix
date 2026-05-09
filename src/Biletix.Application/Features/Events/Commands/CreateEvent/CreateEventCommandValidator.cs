using FluentValidation;

namespace Biletix.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Etkinlik olusturma komutunun giris kurallarini dogrular.
/// </summary>
public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    /// <summary>
    /// Etkinlik olusturma validasyon kurallarini tanimlar.
    /// </summary>
    public CreateEventCommandValidator()
    {
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

        RuleFor(command => command.VenueId)
            .NotEmpty();

        RuleFor(command => command.PerformerId)
            .NotEmpty();

        RuleFor(command => command.TicketTypes)
            .NotEmpty()
            .WithMessage("At least one ticket type is required.");

        RuleForEach(command => command.TicketTypes)
            .ChildRules(ticketType =>
            {
                ticketType.RuleFor(item => item.Name)
                    .NotEmpty()
                    .MaximumLength(100);

                ticketType.RuleFor(item => item.Price)
                    .GreaterThanOrEqualTo(0);

                ticketType.RuleFor(item => item.TotalCapacity)
                    .GreaterThan(0);
            });
    }
}

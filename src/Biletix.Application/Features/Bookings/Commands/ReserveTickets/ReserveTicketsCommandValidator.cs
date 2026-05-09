using FluentValidation;

namespace Biletix.Application.Features.Bookings.Commands.ReserveTickets;

/// <summary>
/// Bilet rezervasyon komutunun giris kurallarini dogrular.
/// </summary>
public sealed class ReserveTicketsCommandValidator : AbstractValidator<ReserveTicketsCommand>
{
    /// <summary>
    /// Rezervasyon validasyon kurallarini tanimlar.
    /// </summary>
    public ReserveTicketsCommandValidator()
    {
        RuleFor(command => command.EventId)
            .NotEmpty();

        RuleFor(command => command.Items)
            .NotEmpty()
            .Must(items => items.Count >= 1)
            .WithMessage("At least one ticket item is required.");

        RuleForEach(command => command.Items)
            .ChildRules(item =>
            {
                item.RuleFor(value => value.TicketTypeId)
                    .NotEmpty();

                item.RuleFor(value => value.Quantity)
                    .InclusiveBetween(1, 10);
            });

        RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(100);
    }
}

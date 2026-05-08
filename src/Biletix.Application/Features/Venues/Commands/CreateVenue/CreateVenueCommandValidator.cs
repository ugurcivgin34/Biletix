using FluentValidation;

namespace Biletix.Application.Features.Venues.Commands.CreateVenue;

/// <summary>
/// Mekan olusturma komutunun giris kurallarini dogrular.
/// </summary>
public sealed class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
{
    /// <summary>
    /// Mekan olusturma validasyon kurallarini tanimlar.
    /// </summary>
    public CreateVenueCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(command => command.Capacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(500000);
    }
}

using FluentValidation;

namespace Biletix.Application.Features.Venues.Commands.UpdateVenue;

/// <summary>
/// Mekan guncelleme komutunun giris kurallarini dogrular.
/// </summary>
public sealed class UpdateVenueCommandValidator : AbstractValidator<UpdateVenueCommand>
{
    /// <summary>
    /// Mekan guncelleme validasyon kurallarini tanimlar.
    /// </summary>
    public UpdateVenueCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

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

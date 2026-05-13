using FluentValidation;

namespace Biletix.Application.Features.Payments.Commands.CreatePaymentIntent;

/// <summary>
/// CreatePaymentIntentCommand giris kurallarini tanimlar.
/// </summary>
public sealed class CreatePaymentIntentCommandValidator : AbstractValidator<CreatePaymentIntentCommand>
{
    /// <summary>
    /// Validator kurallarini olusturur.
    /// </summary>
    public CreatePaymentIntentCommandValidator()
    {
        RuleFor(command => command.BookingId)
            .NotEmpty();
    }
}

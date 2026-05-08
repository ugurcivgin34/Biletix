using FluentValidation;
using MediatR;

namespace Biletix.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline'inda request handler calismadan once FluentValidation kurallarini uygular.
/// </summary>
/// <typeparam name="TRequest">Dogrulanacak request tipi.</typeparam>
/// <typeparam name="TResponse">Request sonucunda donen response tipi.</typeparam>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Request tipi icin kayitli tum validator'lari alir.
    /// </summary>
    /// <param name="validators">Request uzerinde calistirilacak validator listesi.</param>
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Validator yoksa request'i devam ettirir; hata varsa ValidationException firlatir.
    /// </summary>
    /// <param name="request">Dogrulanacak request.</param>
    /// <param name="next">Pipeline'daki sonraki adim veya asil handler.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Pipeline sonucunda uretilen response.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}

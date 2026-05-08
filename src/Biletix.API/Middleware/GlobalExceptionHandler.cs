using System.Text.Json;
using Biletix.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Biletix.API.Middleware;

/// <summary>
/// Uygulamadaki beklenen ve beklenmeyen hatalari standart ProblemDetails cevabina donusturur.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Firlatilan exception tipine gore uygun HTTP durum kodu ve problem cevabi uretir.
    /// </summary>
    /// <param name="httpContext">Istek ve cevap bilgilerini tasiyan HTTP context.</param>
    /// <param name="exception">Yakalanan exception.</param>
    /// <param name="cancellationToken">Asenkron islemi iptal etmek icin kullanilan token.</param>
    /// <returns>Exception'in islenip islenmedigini belirten sonuc.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblemDetails(httpContext, validationException),
            NotFoundException => CreateProblemDetails(httpContext, exception.Message, StatusCodes.Status404NotFound),
            DomainException => CreateProblemDetails(httpContext, exception.Message, StatusCodes.Status422UnprocessableEntity),
            _ => CreateProblemDetails(httpContext, "An unexpected error occurred.", StatusCodes.Status500InternalServerError)
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails, cancellationToken: cancellationToken);

        return true;
    }

    private static ProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ValidationException exception)
    {
        var problemDetails = CreateProblemDetails(
            httpContext,
            "Validation failed.",
            StatusCodes.Status400BadRequest);

        problemDetails.Extensions["errors"] = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return problemDetails;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        string title,
        int statusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = statusCode
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return problemDetails;
    }
}

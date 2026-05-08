using System.Diagnostics;
using Serilog.Context;

namespace Biletix.API.Middleware;

/// <summary>
/// HTTP isteklerinin baslangicini, bitisini, durum kodunu ve calisma suresini loglar.
/// </summary>
public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Request logging icin gerekli logger bagimliligini alir.
    /// </summary>
    /// <param name="logger">HTTP istek loglarini yazacak logger.</param>
    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// HTTP istegini olcer, correlation id ile log context'i zenginlestirir ve sonuc durumuna gore log seviyesi secer.
    /// </summary>
    /// <param name="context">Islenen HTTP isteginin context nesnesi.</param>
    /// <param name="next">Pipeline'daki sonraki middleware.</param>
    /// <returns>Asenkron middleware islemi.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var correlationId = context.Items.TryGetValue("CorrelationId", out var value)
            ? value?.ToString()
            : null;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("HTTP {Method} {Path} started", method, path);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next(context);
                stopwatch.Stop();

                LogCompletion(method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, null);
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

                var statusCode = context.Response.HasStarted
                    ? context.Response.StatusCode
                    : StatusCodes.Status500InternalServerError;

                LogCompletion(method, path, statusCode, stopwatch.ElapsedMilliseconds, exception);
                throw;
            }
        }
    }

    private void LogCompletion(string method, string path, int statusCode, long elapsedMilliseconds, Exception? exception)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                method,
                path,
                statusCode,
                elapsedMilliseconds);

            return;
        }

        if (statusCode >= StatusCodes.Status400BadRequest)
        {
            _logger.LogWarning(
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                method,
                path,
                statusCode,
                elapsedMilliseconds);

            return;
        }

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
            method,
            path,
            statusCode,
            elapsedMilliseconds);
    }
}

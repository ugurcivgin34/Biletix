using Serilog.Context;

namespace Biletix.API.Middleware;

/// <summary>
/// Her HTTP istegine correlation id atar ve bu degeri response header ile log context'e tasir.
/// </summary>
public class CorrelationIdMiddleware : IMiddleware
{
    /// <summary>
    /// Correlation id bilgisinin tasindigi HTTP header adidir.
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Request header'dan correlation id okur; yoksa yeni bir id uretip downstream pipeline'a aktarir.
    /// </summary>
    /// <param name="context">Islenen HTTP isteginin context nesnesi.</param>
    /// <param name="next">Pipeline'daki sonraki middleware.</param>
    /// <returns>Asenkron middleware islemi.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var values)
            ? values.FirstOrDefault()
            : null;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
